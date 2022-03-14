using System.Collections.Generic;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;

namespace WebCrawlerPOC.Services.Kafka.Interfaces
{
    public interface IKafkaMessengerService
    {
        Task<List<KafkaReturnValue>> SendKafkaMessage(string id, string subject, object message, string topic = "");
    }
}
