using System.Collections.Generic;

namespace BuildingGraph.Client.Model
{
    public class Edge
    {
        public virtual string TypeLabel
        {
            get; set;
        }

        public MEPEdgeTypes EdgeType { get; set; }
        public Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();
        public virtual Dictionary<string, object> GetAllProperties()
        {
            var allProps = new Dictionary<string, object>();

            foreach (var kvp in ExtendedProperties)
            {
                allProps.Add(kvp.Key, kvp.Value);
            }

            return allProps;

        }
    }

}
