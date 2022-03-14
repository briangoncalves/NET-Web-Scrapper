using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using WebCrawlerPOC.Services.Interfaces;

namespace WebCrawlerPOC.Controllers
{
    [ApiController]
    public class WebCrawlerController : ControllerBase
    {
        private readonly IHtmlValidators htmlValidators;
        private readonly IHtmlScrapper scrapper;
        private readonly IImageValidators imageValidators;
        private readonly IUrlKafkaSenderService urlKafkaSenderService;

        public WebCrawlerController(IHtmlValidators validators, IHtmlScrapper scrapper, IImageValidators imageValidators
            , IUrlKafkaSenderService urlKafkaSenderService)
        {
            this.htmlValidators = validators;
            this.scrapper = scrapper;
            this.imageValidators = imageValidators;
            this.urlKafkaSenderService = urlKafkaSenderService;
        }

        [HttpGet]
        [Route("/api/web-crawler/get-html")]
        public async Task<IActionResult> GetHtml(string url)
        {
            if (!htmlValidators.CheckURLValid(url)) return BadRequest();
            if (!htmlValidators.CheckURLExists(url)) return BadRequest();

            var html = await scrapper.DownloadHtml(url);

            return Ok(html);
        }

        [HttpGet]
        [Route("/api/web-crawler/get-links-from-url")]
        public async Task<IActionResult> GetLinksFromUrl(string url)
        {
            if (!htmlValidators.CheckURLValid(url)) return BadRequest();
            if (!htmlValidators.CheckURLExists(url)) return BadRequest();

            var html = await scrapper.DownloadHtml(url);
            var links = scrapper.GetListOfUrls(html);

            return Ok(links);
        }

        [HttpGet]
        [Route("/api/web-crawler/get-images-from-url")]
        public async Task<IActionResult> GetImagesFromUrl(string url)
        {
            if (!htmlValidators.CheckURLValid(url)) return BadRequest();
            if (!htmlValidators.CheckURLExists(url)) return BadRequest();

            var html = await scrapper.DownloadHtml(url);
            var links = scrapper.GetListOfImages(html);

            return Ok(links);
        }

        [HttpGet]
        [Route("/api/web-crawler/check-images-from-url")]
        public async Task<IActionResult> CheckImagesFromUrl(string url)
        {
            if (!htmlValidators.CheckURLValid(url)) return BadRequest();
            if (!htmlValidators.CheckURLExists(url)) return BadRequest();

            var html = await scrapper.DownloadHtml(url);
            var links = scrapper.GetListOfImages(html);

            var result = new ConcurrentBag<Tuple<string, bool>>();

            links.AsParallel().ForAll(link =>
            {
                var valid = imageValidators.CheckValidImage(link);
                result.Add(new Tuple<string, bool>(link, valid));
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("/api/web-crawler/start-crawling")]
        public async Task<IActionResult> StartCrawling(string url)
        {
            if (!htmlValidators.CheckURLValid(url)) return BadRequest();
            if (!htmlValidators.CheckURLExists(url)) return BadRequest();

            urlKafkaSenderService.SendEvent(new Domain.UrlValidation
            {
                Url = url
            });

            return Ok();
        }
    }
}
