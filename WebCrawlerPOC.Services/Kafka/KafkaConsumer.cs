using Kafka.Public;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.Services.Kafka
{
    public class KafkaConsumer : IKafkaConsumer
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly IServiceProvider _services;
        public KafkaConsumer(ILogger<KafkaConsumer> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }
        public async Task Consume(RawKafkaRecord record, IMappedServices mappedServices)
        {
            try
            {
                if (record.Key == null)
                {
                    _logger.LogError("Message key cannot be null!");
                }
                else
                {
                    var value = Encoding.UTF8.GetString(record.Value as byte[]);
                    var message = JsonSerializer.Deserialize<KafkaMessage>(value);
                    var messageKey = "";
                    if (record.Key != null)
                    {
                        messageKey = Encoding.UTF8.GetString(record.Key as byte[]);
                        _logger.LogInformation($"{messageKey}");
                    }
                    else
                    {
                        _logger.LogInformation($"Message witout a key being processed");
                    }
                    if (mappedServices.Services.ContainsKey(message.Type))
                    {
                        var type = mappedServices.Services[message.Type];
                        object objMessage = null;
                        // Try to use CamelCase
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            };
                            objMessage = JsonSerializer.Deserialize(message.Data.ToString(), type.MessageType, options);
                        }
                        catch (Exception ex)
                        {
                            // If it is still failing to deserialize the object, log the error
                            _logger.LogError($"Message with key: {messageKey} is not in a valid format.{Environment.NewLine}{ex}");
                            throw;
                        }
                        try
                        {
                            using (var scope = _services.CreateScope())
                            {
                                var service = scope.ServiceProvider.GetRequiredService(type.ServiceType);
                                await (dynamic)service.GetType().GetMethod("Execute").Invoke(service, new object[] { objMessage, message.Subject });
                                _logger.LogInformation($"Message with key: {messageKey} and type: {message.Type} being processed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Message with key: {messageKey} and type: {message.Type} failed to be executed.{Environment.NewLine}{ex}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading the message.{Environment.NewLine}{ex}");
                throw;
            }
        }
    }
}
