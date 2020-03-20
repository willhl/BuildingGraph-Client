using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace BuildingGraph.Client.Neo4j
{
    public delegate void OnCommit(IResultCursor result);

    public class PendingCypher
    {
        public string Query { get; set; }
        public Dictionary<string, object> Props { get; set; }

        public OnCommit Completed { get; set; }

        internal async void Complete(IResultCursor result)
        {
            Completed?.Invoke(result);

        }

    }

}
