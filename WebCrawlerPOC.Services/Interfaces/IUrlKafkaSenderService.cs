using System.Collections.Generic;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;

namespace WebCrawlerPOC.Services.Interfaces
{
    public interface IUrlKafkaSenderService
    {
        Task<UrlValidation> FindByUrlAsync(string url);
        List<KafkaReturnValue> SendEvent(UrlValidation obj);
    }
}