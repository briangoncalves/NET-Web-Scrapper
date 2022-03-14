using System.Collections.Generic;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;
using WebCrawlerPOC.Services.Interfaces;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.Services
{
    public class UrlKafkaSenderService : IUrlKafkaSenderService
    {
        private readonly IMongoRepository<UrlValidation> _urlRepository;
        private readonly IKafkaMessengerService _kafkaMessengerService;
        public UrlKafkaSenderService(IMongoRepository<UrlValidation> urlRepository, IKafkaMessengerService kafkaMessengerService)
        {
            _urlRepository = urlRepository;
            _kafkaMessengerService = kafkaMessengerService;
        }
        public Task<UrlValidation> FindByUrlAsync(string url)
        {
            return _urlRepository.FindOneAsync(b => b.Url == url);
        }

        public List<KafkaReturnValue> SendEvent(UrlValidation obj)
        {
            Task<List<KafkaReturnValue>> tasks;

            using (tasks = _kafkaMessengerService.SendKafkaMessage(obj.Url, "UrlFound", obj))
            {
                tasks.Wait();
            }

            return tasks.Result;
        }
    }
}
