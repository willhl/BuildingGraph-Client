using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Neo4j.Driver;
using BuildingGraph.Client.Model;
using System.Xml.Linq;

namespace BuildingGraph.Client.Neo4j
{

    public class Neo4jClient : IDisposable
    {


        IDriver _driver;
        string _database;
        Dictionary<string, string[]> _labelMapping;

        public Neo4jClient(Uri host, string userName, string password) : this(host, userName, password, string.Empty)
        {
     
        }

        public Neo4jClient(Uri host, string userName, string password, string database)
        {
            _driver = GraphDatabase.Driver(host, AuthTokens.Basic(userName, password));
            _database = database;
            _labelMapping = LabelMap.DefaultMapping;
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
            IAsyncSession session;

            if (!string.IsNullOrEmpty(_database))
            {
                session = _driver.AsyncSession((cf) => cf.WithDatabase(_database));
            }
            else
            {
                session = _driver.AsyncSession();
            }

            try
            {

                var sessionVars = new Dictionary<string, object>();
                sessionVars.Add("TimeStamp", DateTime.Now);
                sessionVars.Add("By", System.Environment.UserName);
                var sessionNode = new Node("Session");
                Push(sessionNode, sessionVars);

                await pushQueue(schemaStack, session, sessionNode.Id);
                await pushQueue(pushStack, session, sessionNode.Id);
                await pushQueue(relateStack, session, sessionNode.Id);

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
                    var rsres = await result.ToListAsync();
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


        private async Task<bool> pushQueue(Queue<PendingCypher> pendingCyphers, IAsyncSession session, string sessionId)
        {

            //var pushTasks = new List<Task>();

            var wtxResult = await session.WriteTransactionAsync(async tx =>
            {
                while (pendingCyphers.Count > 0)
                {
                    var pendingQuery = pendingCyphers.Dequeue();

                    if (pendingQuery.Props != null && pendingQuery.Props.ContainsKey("SessionId"))
                    {
                        pendingQuery.Props["SessionId"] = sessionId;
                    }

                    var result = await (pendingQuery.Props != null && pendingQuery.Props.Count > 0 ? tx.RunAsync(pendingQuery.Query, pendingQuery.Props) : tx.RunAsync(pendingQuery.Query));

                    pendingQuery.Complete(result);

                    //pushTasks.Add(result);
                    // commitedList.Add(pendingQuery);
                }
                return true;
                //return Task.WhenAll(pushTasks);
            });


            return wtxResult;

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

            var pendingNode = new PendingNode(node);
            if (qlSafeVariables.ContainsKey(pendingNode.Id))
            {
                qlSafeVariables.Add("Id", pendingNode.Id);
            }
            else
            {
                qlSafeVariables["Id"] = pendingNode.Id;
            }

            if (!qlSafeVariables.ContainsKey("SessionId"))
            {
                qlSafeVariables.Add("SessionId", "");
            }

            Dictionary<string, object> props = new Dictionary<string, object>();
            props.Add("props", qlSafeVariables);

            //todo: sort out multiple labels
            var nodeLabel = node.Labels.First();
            var allNodeLabels = node.Labels.First();
            if (_labelMapping.ContainsKey(nodeLabel))
            {
                allNodeLabels = nodeLabel + ":" + string.Join(":", _labelMapping[nodeLabel]);
            }

            var query = string.Format("CREATE (nn:{0} $props)", allNodeLabels);

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
