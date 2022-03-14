using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using WebCrawlerPOC.Infrastructure;
using WebCrawlerPOC.Infrastructure.Interfaces;
using WebCrawlerPOC.Infrastructure.Mongo;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;
using WebCrawlerPOC.Services;
using WebCrawlerPOC.Services.Interfaces;
using WebCrawlerPOC.Services.Kafka;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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

            var KafkaHost = Environment.GetEnvironmentVariable("KAFKA_HOST");
            var KafkaTopic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            var sender = new List<KafkaSender>
                    {
                        new KafkaSender
                        {
                            Topic = KafkaTopic
                        }
                    };
            services.AddSingleton<IKafkaConfig>(kc =>
                new KafkaConfig() { Host = KafkaHost, Sender = sender }
            );
            services.AddScoped(p => new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = KafkaHost
            }).Build());
            services.AddTransient<IKafkaSender, KafkaSender>();
            services.AddTransient<IKafkaMessengerService, KafkaMessengerService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebCrawlerPOC", Version = "v1" });
            });
            services.AddTransient<IHtmlScrapper, HtmlScrapper>();
            services.AddTransient<IHtmlValidators, HtmlValidators>();
            services.AddTransient<IImageValidators, ImageValidators>();
            services.AddTransient<IUrlKafkaSenderService, UrlKafkaSenderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebCrawlerPOC v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
