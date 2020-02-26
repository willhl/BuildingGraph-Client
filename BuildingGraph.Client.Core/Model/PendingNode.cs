using System.Collections.Generic;

namespace BuildingGraph.Client
{
    public class PendingNode
    {
        public PendingNode(Model.Node node)
        {
            Id = shortid.ShortId.Generate(7);
            NodeName = node.Label;
        }

        public PendingNode(string nodeName)
        {
            Id = shortid.ShortId.Generate(7);
            NodeName = nodeName;
        }
        public PendingNode(string nodeName, string id)
        {
            Id = id;
            NodeName = nodeName;
            WasCommited = !string.IsNullOrEmpty(id);
        }


        public void SetCommited(string Id)
        {
            if (!string.IsNullOrEmpty(Id)) this.Id = Id;
            WasCommited = true;
        }


        public string Id { get; internal set; }

        //node no longer required? can just use NodeName
        public string NodeName { get; internal set; }

        public bool WasCommited { get; internal set; }

        public Dictionary<string, object> Variables { get; internal set; }

        public string MutationAlias { get; set; }
    }
}
