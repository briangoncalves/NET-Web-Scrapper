using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Infrastructure;
using WebCrawlerPOC.Infrastructure.Interfaces;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.Services.Kafka
{
    public class KafkaMessengerService : IKafkaMessengerService
    {
        private readonly IKafkaConfig _kafkaConfig;
        private readonly IProducer<string, string> _producer;

        public KafkaMessengerService(IKafkaConfig kafkaConfig, IProducer<string, string> producer)
        {
            _kafkaConfig = kafkaConfig;
            _producer = producer;
        }

        public async Task<List<KafkaReturnValue>> SendKafkaMessage(string id, string subject, object message, string topic = "")
        {
            var deliveryResults = new List<KafkaReturnValue>();
            var senders = new List<IKafkaSender>();
            if (!string.IsNullOrEmpty(topic))
            {
                senders.Add(
                new KafkaSender
                {
                    Topic = topic
                });
            }
            else
            {
                senders.AddRange(_kafkaConfig.Sender);
            }
            foreach (var sender in senders)
            {
                var result = await Send(id, subject, message, sender.Topic);
                deliveryResults.Add(result);
            }

            _producer.Flush(TimeSpan.FromSeconds(30));

            return deliveryResults;
        }

        private async Task<KafkaReturnValue> Send(string id, string subject, object message, string topic)
        {
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                var jsonMessage = JsonSerializer.Serialize(message, options);

                try
                {
                    var result = await _producer.ProduceAsync(topic, new Message<string, string>()
                    {
                        Key = id,
                        Value = JsonSerializer.Serialize(new KafkaMessage
                        {
                            Type = message.GetType().Name,
                            Data = jsonMessage,
                            DataContentType = "application/json",
                            Time = DateTime.UtcNow,
                            SpecVersion = "1.0",
                            Id = id,
                            Source = _kafkaConfig.Source,
                            Subject = subject
                        })
                    });

                    return new KafkaReturnValue
                    {
                        Status = ReturnStatus.Success,
                        DeliveryResult = result
                    };
                }
                catch (Exception ex)
                {
                    return new KafkaReturnValue
                    {
                        Status = ReturnStatus.ErrorUnknown,
                        Exception = ex
                    };
                }
            };
        }
    }
}
