using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawlerPOC.Infrastructure.Interfaces;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.HostedService
{
    public class KafkaHostedService : IHostedService
    {
        private readonly object _lockObject = new();
        private readonly ClusterClient _cluster;
        private readonly IKafkaConfig _kafkaConfig;
        private readonly IMappedServices _mappedServices;
        private readonly IKafkaConsumer _consumer;
        private readonly ILogger<KafkaHostedService> _logger;

        public KafkaHostedService(IKafkaConfig kafkaConfig, IMappedServices mappedServices,
            IKafkaConsumer consumer,
            ILogger<KafkaHostedService> logger)
        {
            _logger = logger;
            _kafkaConfig = kafkaConfig;
            _cluster = new ClusterClient(new Configuration
            {
                Seeds = _kafkaConfig.Host
            }, new ConsoleLogger());

            _mappedServices = mappedServices;
            _consumer = consumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"KafkaHostedService - StartAsync() invoked at {DateTime.Now:G}");

            if (Monitor.TryEnter(_lockObject))
            {
                try
                {

                    _kafkaConfig.Listeners.ToList().ForEach(listener =>
                    {
                        _cluster.Subscribe(listener.ConsumerGroupId, listener.Topics,
                        new ConsumerGroupConfiguration
                        {
                            AutoCommitEveryMs = 5000
                        });
                        _cluster.MessageReceived += ClusterMessageReceived;
                    });

                }
                catch (Exception ex)
                {
                    _logger.LogError($"KafkaHostedService - StartAsync() threw Exception{Environment.NewLine}{ex}");
                }
            }

            return Task.CompletedTask;
        }

        private void ClusterMessageReceived(RawKafkaRecord record)
        {
            try
            {
                _consumer.Consume(record, _mappedServices);
            }
            catch (Exception ex)
            {
                _logger.LogError($"KafkaHostedService - ClusterMessageReceived() threw Exception{Environment.NewLine}{ex}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"KafkaHostedService - StopAsync() invoked at {DateTime.Now:G}");

            _cluster?.Dispose();
            return Task.CompletedTask;
        }
    }
}
