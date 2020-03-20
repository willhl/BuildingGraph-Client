using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BuildingGraph.Client.Model;

namespace BuildingGraph.Client.Neo4j.Tests
{


    [TestClass]
    public class LoadTests
    {
        [TestMethod]
        public async Task CreateNodesLoadTest()
        {
            Neo4jClient n4 = new Neo4jClient(new Uri("bolt://localhost:8687"), "neo4j", "neo4j");

            Node b = new Node("Building");
            b.ExtendedProperties.Add("Name", "Building 1");
            var pbn = n4.Push(b, b.ExtendedProperties);

            var pnl = new List<PendingNode>();

            for (int i = 1; i < 100; i++)
            {
                Node n = new Node("Space");
                n.ExtendedProperties.Add("Name", "Space " + i);

                for (int p = 1; p < 100; p++)
                {
                    n.ExtendedProperties.Add("Param " + p, p);
                }

                var spn = n4.Push(n, n.ExtendedProperties);
                pnl.Add(spn);
                n4.Relate(spn, pbn, "IS_IN", null);

                for (int e = 1; e < 100; e++)
                {
                    Node en = new Node("Element");
                    en.ExtendedProperties.Add("Name", "Element " + i);

                    for (int p = 1; p < 100; p++)
                    {
                        en.ExtendedProperties.Add("Param " + p, p);
                    }

                    var epn = n4.Push(en, en.ExtendedProperties);
                    pnl.Add(epn);
                    n4.Relate(epn, spn, "IS_IN", null);
                }


            }

            await n4.CommitAsync();

        }
    }
}
