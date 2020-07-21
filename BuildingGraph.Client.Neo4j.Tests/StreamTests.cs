using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BuildingGraph.Client.Kafka;
using System.Diagnostics;

namespace BuildingGraph.Client.Neo4j.Tests
{
    [TestClass]
    public class StreamTests
    {
        [TestMethod]
        public void Connect()
        {

            KafkaConsumer kc = new KafkaConsumer("localhost:9092");


            kc.NewMessageArrived += Kc_NewMessageArrived;

            kc.Start("lc-ng", new string[] { "ext-service-abstractElements" });
        }

        private void Kc_NewMessageArrived(StreamMessage message)
        {
            var nonde = message.After.AsNode();
            var id = nonde.Id;

        }
    }
}
