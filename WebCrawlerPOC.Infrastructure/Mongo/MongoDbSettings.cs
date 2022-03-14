using MongoDB.Driver;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;

namespace WebCrawlerPOC.Infrastructure.Mongo
{
    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CertificatePath { get; set; }
        public string CertificateHash { get; set; }
        public IMongoClient Client
        {
            private set { _client = value; }
            get
            {
                if (_client == null)
                    InitializeIMongoClient();
                return _client;
            }
        }
        private IMongoClient _client;

        private void InitializeIMongoClient()
        {
            if (!string.IsNullOrEmpty(CertificatePath))
            {
                var settings = SetupSettingsMongo();
                Client = new MongoClient(settings);
                return;
            }
            if (ConnectionString != null)
            {
                Client = new MongoClient(ConnectionString);
            }
            else
            {
                Client = new MongoClient();
            }

        }

        public MongoDbSettings()
        {
            Client = null;
        }

        MongoClientSettings SetupSettingsMongo()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
            settings.SslSettings = new SslSettings
            {
                EnabledSslProtocols = SslProtocols.Tls12,
                ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return certificate.GetCertHashString() == CertificateHash;
                }
            };
            return settings;
        }

        public IMongoClient CreateClient()
        {
            InitializeIMongoClient();
            return Client;
        }
    }
}