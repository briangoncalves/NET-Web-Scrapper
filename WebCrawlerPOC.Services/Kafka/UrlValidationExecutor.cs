using System;
using System.Linq;
using System.Threading.Tasks;
using WebCrawlerPOC.Domain;
using WebCrawlerPOC.Infrastructure.Mongo.Interfaces;
using WebCrawlerPOC.Services.Interfaces;
using WebCrawlerPOC.Services.Kafka.Interfaces;

namespace WebCrawlerPOC.Services.Kafka
{
    public class UrlValidationExecutor : IKafkaExecutor<UrlValidation>
    {
        private readonly IUrlKafkaSenderService _urlService;
        private readonly IHtmlValidators _validator;
        private readonly IHtmlScrapper _scrapper;
        private readonly IMongoRepository<UrlValidation> _repository;
        private const int EXPIRE_DAY = 7;

        public UrlValidationExecutor(IUrlKafkaSenderService urlService, IMongoRepository<UrlValidation> repository, IHtmlValidators validator
            , IHtmlScrapper scrapper)
        {
            _urlService = urlService;
            _repository = repository;
            _validator = validator;
            _scrapper = scrapper;
        }

        public async Task<bool> Execute(UrlValidation obj, string subject)
        {
            var found = await _urlService.FindByUrlAsync(obj.Url);
            if (found is null)
            {
                if (Validate(obj))
                {
                    await GenerateListOfUrls(obj);
                }

                await _repository.InsertOneAsync(obj);
            }
            else
            {
                if (found.ExpireDate.AddDays(EXPIRE_DAY) >= DateTime.UtcNow) return true;
                if (Validate(obj))
                {
                    await GenerateListOfUrls(obj);
                }

                found.ExpireDate = obj.ExpireDate;
                found.Valid = obj.Valid;
                await _repository.ReplaceOneAsync(found);
            }
            return true;
        }

        private async Task GenerateListOfUrls(UrlValidation obj)
        {
            var html = await _scrapper.DownloadHtml(obj.Url);
            var urls = _scrapper.GetListOfUrls(html);

            urls.ToList().ForEach(url =>
            {
                _urlService.SendEvent(new UrlValidation()
                {
                    Url = url
                });
            });
        }

        private bool Validate(UrlValidation obj)
        {
            var valid = _validator.CheckURLValid(obj.Url) && _validator.CheckURLExists(obj.Url);
            obj.ExpireDate = DateTime.UtcNow.AddDays(EXPIRE_DAY);
            obj.Valid = valid;
            return valid;
        }
    }
}
