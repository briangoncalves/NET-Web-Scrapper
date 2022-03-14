using MongoDB.Driver;

namespace WebCrawlerPOC.Infrastructure.Mongo.Interfaces
{
    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string CertificatePath { get; set; }
        string CertificateHash { get; set; }
        IMongoClient CreateClient();
    }
}
