using System.Collections.Generic;
using Neo4j.Driver;


namespace BuildingGraph.Client.Neo4j
{
    delegate void OnCommit(IResultCursor result);

    class PendingCypher
    {
        public string Query { get; set; }
        public Dictionary<string, object> Props { get; set; }

        public OnCommit Committed { get; set; }
        public Model.Node Node { get; set; }

        public PendingNode FromNode { get; set; }
        public PendingNode ToNode { get; set; }
        public string RelType { get; set; }

        internal async void NodeCommited(IResultCursor result)
        {
            Committed?.Invoke(result);

        }

    }
}
