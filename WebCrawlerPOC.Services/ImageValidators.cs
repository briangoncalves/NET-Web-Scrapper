using SixLabors.ImageSharp;
using System.IO;
using WebCrawlerPOC.Services.Interfaces;

namespace WebCrawlerPOC.Services
{
    public class ImageValidators : IImageValidators
    {
        public bool CheckValidImage(string url)
        {
            try
            {
                var imageStream = DownloadImage(url);
                var imageInfo = Image.Identify(imageStream);

                if (imageInfo == null) return false;

                if (imageInfo.Width > 0 && imageInfo.Height > 0) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }

        private MemoryStream DownloadImage(string url)
        {
            if (url.StartsWith("//")) url = "https:" + url;

            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }
    }
}
