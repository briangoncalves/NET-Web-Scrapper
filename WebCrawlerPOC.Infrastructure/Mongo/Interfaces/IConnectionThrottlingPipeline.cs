using System.Threading.Tasks;

namespace WebCrawlerPOC.Infrastructure.Mongo.Interfaces
{
    public interface IConnectionThrottlingPipeline
    {
        Task<T> AddRequest<T>(Task<T> task);
        Task AddRequest(Task task);
    }
}
