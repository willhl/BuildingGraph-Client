using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Neo4j.Driver;
using BuildingGraph.Client.Model;


namespace BuildingGraph.Client.Neo4j
{


    public class Neo4jClient : IDisposable
    {


        IDriver _driver;
        public Neo4jClient(Uri host, string userName, string password)
        {
            _driver = GraphDatabase.Driver(host, AuthTokens.Basic(userName, password));
        }

        HashSet<string> constrained = new HashSet<string>();
        Queue<PendingCypher> schemaStack = new Queue<PendingCypher>();
        Queue<PendingCypher> pushStack = new Queue<PendingCypher>();
        Queue<PendingCypher> relateStack = new Queue<PendingCypher>();
        List<PendingCypher> commitedList = new List<PendingCypher>();
        bool _applyIDConstraints = false;

        public async Task VerifyConnectionAsync()
        {
            await _driver.VerifyConnectivityAsync();
        }

        public async Task CloseAsync()
        {
            await _driver.CloseAsync();
        }

        public async Task CommitAsync()
        {
            var session = _driver.AsyncSession();

            try
            {
               
                await pushQueue(schemaStack, session);
                await pushQueue(pushStack, session);
                await pushQueue(relateStack, session);
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<QueryResults> RunCypherQueryAsync(string query, Dictionary<string, object> props)
        {

            QueryResults res = new QueryResults("New Query");
            
            var session = _driver.AsyncSession();
            try
            {
                var wtxResult = await session.WriteTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync(query, props);
                    var rsres  = await result.ToListAsync();  
                    return rsres;
                });

                res = new QueryResults(wtxResult);
            }
            finally
            {
                await session.CloseAsync();
            }

            return res;
        }


        private async Task<bool> pushQueue(Queue<PendingCypher> pendingCyphers, IAsyncSession session)
        {

            //var pushTasks = new List<Task>();

            var wtxResult = session.WriteTransactionAsync(async tx =>
            {
                while (pendingCyphers.Count > 0)
                {
                    var pendingQuery = pendingCyphers.Dequeue();
                    var result = await (pendingQuery.Props != null && pendingQuery.Props.Count > 0 ? tx.RunAsync(pendingQuery.Query, pendingQuery.Props) : tx.RunAsync(pendingQuery.Query));

                    pendingQuery.Complete(result);
                
                    //pushTasks.Add(result);
                   // commitedList.Add(pendingQuery);
                }
                //return Task.WhenAll(pushTasks);
            });


            await wtxResult;

            return true;

        }

        /// <summary>
        ///  MATCH(a),(b)
        ///  WHERE ID(a) = $fromNodeId AND ID(b) = $toNodeId
        ///  CREATE (a)-[r: $relType $variables ]->(b)
        /// </summary>
        /// <param name="fromNodeId"></param>
        /// <param name="toNodeId"></param>
        /// <param name="relType"></param>
        /// <param name="variables"></param>
        public void Relate(PendingNode fromNode, PendingNode toNode, string relType, Dictionary<string, object> variables)
        {

            var qlSafeVariables = new Dictionary<string, object>();

            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    var qlSafeName = Utils.GetGraphQLCompatibleFieldName(kvp.Key);
                    if (!qlSafeVariables.ContainsKey(qlSafeName))
                    {
                        qlSafeVariables.Add(qlSafeName, Convert.ToNeo4jType(kvp.Value));
                    }
                }
            }


            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("frid", fromNode.Id);
            props.Add("toid", toNode.Id);

            string query = string.Empty;
            if (qlSafeVariables != null && qlSafeVariables.Count > 0)
            {
                props.Add("cvar", qlSafeVariables);
                query =
                    string.Format("MATCH(a: {0} {{Id: $frid}}),(b:{1} {{Id: $toid}})", fromNode.NodeName, toNode.NodeName) +
                    string.Format("CREATE (a)-[r:{0} $cvar]->(b) ", relType);
            }
            else
            {
                query =
                    string.Format("MATCH(a: {0} {{Id: $frid}}),(b:{1} {{Id: $toid}})", fromNode.NodeName, toNode.NodeName) +
                    string.Format("CREATE (a)-[r:{0}]->(b) ", relType);
            }

            var pec = new PendingNodeRelate();
            pec.Query = query;
            pec.Props = props;

            pec.Completed = (IResultCursor result) =>
            {
                var rs = result;
            };

            pec.FromNode = fromNode;
            pec.ToNode = toNode;
            pec.RelType = relType;

            relateStack.Enqueue(pec);

        }




        public PendingNode Push(Node node, Dictionary<string, object> variables)
        {
            var qlSafeVariables = new Dictionary<string, object>();

            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    var qlSafeName = Utils.GetGraphQLCompatibleFieldName(kvp.Key);
                    if (!qlSafeVariables.ContainsKey(qlSafeName))
                    {
                        qlSafeVariables.Add(qlSafeName, Convert.ToNeo4jType(kvp.Value));
                    }
                }
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("props", qlSafeVariables);

            var pendingNode = new PendingNode(node);
            if (qlSafeVariables.ContainsKey(pendingNode.Id))
            {
                qlSafeVariables.Add("Id", pendingNode.Id);
            }
            else
            {
                qlSafeVariables["Id"] = pendingNode.Id;
            }

            var nodeLabel = node.Label;
            var query = string.Format("CREATE (nn:{0} $props)", nodeLabel);

            
            if (_applyIDConstraints && !constrained.Contains(nodeLabel))
            {
                var pecCs = new PendingNodePush();
                pecCs.Query = string.Format("CREATE CONSTRAINT ON(nc:{0}) ASSERT nc.Id IS UNIQUE", nodeLabel);
                schemaStack.Enqueue(pecCs);
                constrained.Add(nodeLabel);
            }

            var pec = new PendingNodePush();
            pec.Query = query;
            pec.Props = props;
            pec.Node = node;
            pec.PushNode = pendingNode;
            pushStack.Enqueue(pec);

            pec.Completed = (IResultCursor result) =>
            {
                var rs = result;
                if (pec.PushNode != null)
                {
                    pec.PushNode.SetCommited(pec.PushNode.Id);
                }

            };

            return pendingNode;

        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_driver != null) _driver.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

    }
}
