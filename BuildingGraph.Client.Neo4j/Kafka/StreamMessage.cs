using System;


namespace BuildingGraph.Client.Kafka
{
    public class StreamMessage
    {

        internal StreamMessage(string neo4jMessage)
        {

            dynamic jsonMsg = Newtonsoft.Json.JsonConvert.DeserializeObject(neo4jMessage);

            if (jsonMsg == null) throw new Exception("message did not deserialize");

            if (jsonMsg.meta != null && jsonMsg.meta.operation != null)
            {
                switch (jsonMsg.meta.operation.Value)
                {
                    case "created":
                        Operation = MessageOperation.Created;
                        break;

                    case "updated":
                        Operation = MessageOperation.Updated;
                        break;

                    case "deleted":
                        Operation = MessageOperation.Deleted;
                        break;
                }

                Meta = jsonMsg.meta;

                
            }

            if (jsonMsg.payload != null)
            {
                if (jsonMsg.payload.before != null) Before = new PayloadContent(jsonMsg.payload.before);

                if (jsonMsg.payload.after != null) After = new PayloadContent(jsonMsg.payload.after);

                payloadType = jsonMsg.payload.type;
            }
        }

        public MessageOperation Operation { get; private set; }

        public PayloadContent Before { get; private set; }
        public PayloadContent After { get; private set; }
        public PayloadContent Schema { get; private set; }
        public dynamic Meta { get; private set; }

        public string payloadType { get; private set; }

    }
}
