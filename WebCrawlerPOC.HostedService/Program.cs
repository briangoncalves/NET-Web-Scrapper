using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Infrastructure;
using WebCrawlerPOC.Infrastructure.Interfaces;
using WebCrawlerPOC.Infrastructure.Mongo;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;
using WebCrawlerPOC.Services;
using WebCrawlerPOC.Services.Interfaces;
using WebCrawlerPOC.Services.Kafka;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.HostedService
{
    public class Program
    {
        private readonly static Dictionary<string, MessageTypeServiceMap> mappedServices = new();
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder().Build();

        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var KafkaHost = Environment.GetEnvironmentVariable("KAFKA_HOST");
            var KafkaConsumerGroup = Environment.GetEnvironmentVariable("KAFKA_CONSUMER_GROUP");
            var KafkaTopic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");

            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {

                    var dbSettings = new MongoDbSettings()
                    {
                        ConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING"),
                        DatabaseName = Environment.GetEnvironmentVariable("MONGO_DB_NAME")
                    };
                    services.AddSingleton<IMongoDbSettings>(_ => dbSettings);
                    services.AddTransient<IMongoClient>(_ => dbSettings.CreateClient());
                    services.AddScoped<IConnectionThrottlingPipeline, ConnectionThrottlingPipeline>();
                    services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

                    var listerners = new List<KafkaListener>
                    {
                        new KafkaListener
                        {
                            ConsumerGroupId = KafkaConsumerGroup,
                            Topics = KafkaTopic.Split(",")
                        }
                    };
                    var sender = new List<KafkaSender>
                    {
                        new KafkaSender
                        {
                            Topic = KafkaTopic
                        }
                    };
                    services.AddSingleton<IKafkaConfig>(kc =>
                        new KafkaConfig() { Host = KafkaHost, Listeners = listerners, Sender = sender }
                    );
                    services.AddScoped(p => new ProducerBuilder<string, string>(new ProducerConfig
                    {
                        BootstrapServers = KafkaHost
                    }).Build());

                    services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
                    services.AddTransient<IKafkaSender, KafkaSender>();
                    services.AddTransient<IKafkaMessengerService, KafkaMessengerService>();
                    services.AddTransient<IUrlKafkaSenderService, UrlKafkaSenderService>();
                    services.AddTransient<IHtmlScrapper, HtmlScrapper>();
                    services.AddTransient<IHtmlValidators, HtmlValidators>();
                    services.AddTransient<IImageValidators, ImageValidators>();
                    AddMappedServices<UrlValidation, UrlValidationExecutor>(services);
                    services.AddSingleton<IMappedServices>(mp => new MappedServices() { Services = mappedServices });
                    services.AddLogging();
                    services.AddHostedService<KafkaHostedService>();
                });
        }

        private static void AddMappedServices<TModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TExecutor>(IServiceCollection services)
            where TModel : class
            where TExecutor : class, IKafkaExecutor<TModel>
        {
            services.AddScoped<IKafkaExecutor<TModel>, TExecutor>();
            mappedServices.Add(typeof(TModel).Name, new MessageTypeServiceMap
            {
                MessageType = typeof(TModel),
                ServiceType = typeof(IKafkaExecutor<TModel>)
            });
        }
    }
}
