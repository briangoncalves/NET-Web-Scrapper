using Kafka.Public;
using System.Threading.Tasks;

namespace WebCrawlerPOC.Services.Kafka.Interfaces
{
    public interface IKafkaConsumer
    {
        Task Consume(RawKafkaRecord record, IMappedServices mappedServices);
    }
}
