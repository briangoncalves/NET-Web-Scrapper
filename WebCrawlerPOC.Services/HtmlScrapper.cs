using HtmlAgilityPack;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawlerPOC.Services.Interfaces;

namespace WebCrawlerPOC.Services
{
    public class HtmlScrapper : IHtmlScrapper
    {
        private readonly IHtmlValidators validators;

        public HtmlScrapper(IHtmlValidators validators)
        {
            this.validators = validators;
        }

        public async Task<string> DownloadHtml(string url)
        {
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(
                new LaunchOptions { Headless = true });
            using (var page = await browser.NewPageAsync())
            {
                await page.GoToAsync(url);
                await WaitPageRender(page, 30000, url);
                var result = await page.GetContentAsync();
                return result;
            }
        }

        private async Task<bool> WaitPageRender(Page page, int timeout, string originalUrl)
        {
            var checkDurationMsecs = 1000;
            var maxChecks = timeout / checkDurationMsecs;
            var lastHTMLSize = 0;
            var checkCounts = 1;
            var countStableSizeIterations = 0;
            var minStableSizeIterations = 3;

            while (checkCounts++ <= maxChecks)
            {
                if (originalUrl != page.Url)
                    await page.WaitForNavigationAsync();
                var html = await page.GetContentAsync();
                var currentHTMLSize = html.Length;
                if (lastHTMLSize != 0 && currentHTMLSize == lastHTMLSize)
                    countStableSizeIterations++;
                else
                    countStableSizeIterations = 0;

                if (countStableSizeIterations >= minStableSizeIterations)
                {
                    return true;
                }

                lastHTMLSize = currentHTMLSize;
                await page.WaitForTimeoutAsync(checkDurationMsecs);
            }
            return false;
        }

        public IEnumerable<string> GetListOfUrls(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var links = new List<string>();
            foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]").Distinct())
            {
                var hrefValue = link.GetAttributeValue("href", string.Empty);
                if (validators.CheckURLValid(hrefValue))
                    links.Add(hrefValue);
            }
            return links;
        }

        public IEnumerable<string> GetListOfImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var images = doc.DocumentNode.SelectNodes("//img[@src]")
                                .Where(e => e.GetAttributes("hp-lazy-src") == null
                                    && !string.IsNullOrEmpty(e.GetAttributeValue("src", null))
                                    )
                                .Select(e => e.GetAttributeValue("src", null))
                                .Distinct();
            var imagesLazy = doc.DocumentNode.SelectNodes("//img[@hp-lazy-src]")
                                .Where(e => e.GetAttributes("hp-lazy-src") != null
                                    && !string.IsNullOrEmpty(e.GetAttributeValue("hp-lazy-src", null))
                                    )
                                .Select(e => e.GetAttributeValue("hp-lazy-src", null))
                                .Distinct();
            return images.Concat(imagesLazy);
        }
    }
}
