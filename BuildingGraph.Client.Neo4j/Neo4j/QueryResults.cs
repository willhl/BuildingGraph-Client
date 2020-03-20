using System.Collections.Generic;
using Neo4j.Driver;
using Newtonsoft.Json;


namespace BuildingGraph.Client.Neo4j
{
    public class QueryResults
    {
        internal QueryResults(string messages)
        {
            Messages = messages;
        }

        internal QueryResults(List<IRecord> results)
        {
            AsJson = JsonConvert.SerializeObject(results);
        }

        public string Messages { get; internal set; }
        public string AsJson { get; internal set; }
    }
}
