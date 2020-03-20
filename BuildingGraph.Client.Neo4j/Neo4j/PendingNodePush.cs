namespace BuildingGraph.Client.Neo4j
{
    public class PendingNodePush : PendingCypher
    {
        public Model.Node Node { get; set; }
        public PendingNode PushNode { get; set; }
    }
}
