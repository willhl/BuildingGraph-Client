namespace BuildingGraph.Client.Neo4j
{
    public class PendingNodeRelate : PendingCypher
    {
        public Model.Node Node { get; set; }
        public PendingNode FromNode { get; set; }
        public PendingNode ToNode { get; set; }
        public string RelType { get; set; }

    }
}
