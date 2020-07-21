using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Confluent.Kafka;
using Newtonsoft.Json;
using BuildingGraph.Client.Model;
using System.Security.Permissions;

namespace BuildingGraph.Client.Kafka
{


    public delegate void ConsumeKafkaMessageEvent(StreamMessage message);

    public class KafkaConsumer
    {

        string _bootstrapServers;
        public KafkaConsumer(string bootstrapServers)
        {

            _bootstrapServers = bootstrapServers;
        }

        bool cancelled = false; 
        public void Start(string group, ICollection<string> topics)
        {

            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = group,
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            var ct = new CancellationToken();
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(topics);

                while (!cancelled)
                {
                    var consumeResult = consumer.Consume(ct);

                    // handle consumed message

                    var msg = new StreamMessage(consumeResult.Message.Value);

                    NewMessageArrived?.Invoke(msg);

                }

                consumer.Close();
            }

        }

        public async Task StartAsync(string group, ICollection<string> topics)
        {
            await Task.Run(() => Start(group, topics));
        }

        public void Stop()
        {
            cancelled = true;
        }

        public event ConsumeKafkaMessageEvent NewMessageArrived;
    }
}
