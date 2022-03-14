using System.Collections.Generic;
using WebCrawlerPOC.Domain;

namespace WebCrawlerPOC.Services.Kafka.Interfaces
{
    public interface IMappedServices
    {
        Dictionary<string, MessageTypeServiceMap> Services { get; set; }
    }
}
