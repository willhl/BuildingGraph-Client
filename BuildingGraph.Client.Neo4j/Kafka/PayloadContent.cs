using System.Collections.Generic;
using BuildingGraph.Client.Model;


namespace BuildingGraph.Client.Kafka
{
    public class PayloadContent
    {

        internal PayloadContent(dynamic content)
        {
            //parse dynamic to objects

            //parse labels
            // Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>()
            if (content.labels != null) Lables = content.labels.ToObject<string[]>();

            if (content.properties != null) Properties = content.properties.ToObject<Dictionary<string, object>>();
        }

        public Dictionary<string, object> Properties { get; private set; }
        public ICollection<string> Lables { get; private set; }

        public Node AsNode()
        {
            var anode = new Node(Lables);

            foreach (var kvp in Properties)
            {
                anode.ExtendedProperties.Add(kvp.Key, kvp.Value);
                if (kvp.Key == "Id") anode.Id = (string)kvp.Value;
            }

            return anode;
        }
    }
}
