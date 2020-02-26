using System.Collections.Generic;

//using System.Net.Http;
using BuildingGraph.Client.Introspection;

namespace BuildingGraph.Client
{
    internal class MutationRequest
    {
        public MutationRequest(IBuildingGraphField mutation, PendingNode node, Dictionary<string, object> variables)
        {
            Variables = variables;
            Mutation = mutation;
            Nodes = new List<PendingNode>();
            Nodes.Add(node);
        }

        public Dictionary<string, object> Variables { get; }
        public List<PendingNode> Nodes { get; }
        public IBuildingGraphField Mutation { get; }
        public string ReturnFields { get; set; }
        public string MutationAlias { get; set; }
        public string DB_UUID
        {
            get; set;
        }
        public string Query { get; set; }
        public bool IsComplete { get; set; }
    }

}
