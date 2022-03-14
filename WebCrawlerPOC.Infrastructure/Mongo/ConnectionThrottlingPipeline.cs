using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;

namespace WebCrawlerPOC.Infrastructure.Mongo
{
    public class ConnectionThrottlingPipeline : IConnectionThrottlingPipeline
    {
        private readonly Semaphore openConnectionSemaphore;

        public ConnectionThrottlingPipeline(IMongoClient client)
        {
            // We reserve 10 connections in case we have proccess that does not use the Semaphore
            var reservedConnections = 10;
            if (client.Settings.MaxConnectionPoolSize > reservedConnections)
            {
                openConnectionSemaphore = new Semaphore(client.Settings.MaxConnectionPoolSize - reservedConnections,
                    client.Settings.MaxConnectionPoolSize - reservedConnections);
            }
        }

        public async Task<T> AddRequest<T>(Task<T> task)
        {
            openConnectionSemaphore.WaitOne();
            try
            {
                var result = await task;
                return result;
            }
            finally
            {
                openConnectionSemaphore.Release();
            }
        }

        public async Task AddRequest(Task task)
        {
            openConnectionSemaphore.WaitOne();
            try
            {
                await task;
            }
            finally
            {
                openConnectionSemaphore.Release();
            }
        }
    }
}