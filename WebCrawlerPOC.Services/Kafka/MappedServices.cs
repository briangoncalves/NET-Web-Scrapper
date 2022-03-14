using System.Collections.Generic;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.Services.Kafka
{
    public class MappedServices : IMappedServices
    {
        public Dictionary<string, MessageTypeServiceMap> Services { get; set; }
    }
}
