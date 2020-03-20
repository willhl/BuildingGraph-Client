using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;


namespace BuildingGraph.Client.Neo4j.Tests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public async Task spquery()
        {

            Neo4jClient n4 = new Neo4jClient(new Uri("bolt://localhost:8687"), "neo4j", "neo4j");

            var q1 = @"match (frmSpace:Space {Number:$fromSpaceNumber}) MATCH (toSpace:Space {Number:$toSpaceNumber})
CALL apoc.algo.dijkstra(frmSpace, toSpace, 'TRAVEL_TO', 'Distance') YIELD path, weight
with nodes(path) as sfs, path, weight, frmSpace, toSpace
unwind sfs as sf
match p=(n)-[b:BOUNDED_BY]-(sf:Surface)-[o:IS_ON]-(t)
RETURN path, sum(weight) as totalDistance, collect(p) as context";


            var q2 = @"match (sp:Space {Number:$fromSpaceNumber}) MATCH (sc:Space {Number:$toSpaceNumber})
CALL apoc.algo.dijkstra(sp, sc, 'TRAVEL_TO', 'Distance') YIELD path, weight
RETURN path, sum(weight)";

            var vars = new Dictionary<string, object>();
            vars.Add("fromSpaceNumber", "01-01");
            vars.Add("toSpaceNumber", "02-01");
            var res = await n4.RunCypherQueryAsync(q1, vars);

            _ = res.Messages;
        }


    }
}
