using System.Threading.Tasks;

namespace WebCrawlerPOC.Services.Kafka.Interfaces
{
    public interface IKafkaExecutor<T> where T : class
    {
        Task<bool> Execute(T message, string subject);
    }
}
