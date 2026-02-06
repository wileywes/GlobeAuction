using System;
using System.IO;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class ImageHelper
    {
        private readonly HttpServerUtilityBase _server;

        public ImageHelper(HttpServerUtilityBase server)
        {
            _server = server;
        }

        public string SaveItemImageAndGetUrl(HttpPostedFileBase file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

            // Prefix with date (YYYYMMDD)
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var baseFileName = $"{datePrefix}_{nameWithoutExt}";

            var imagesDir = _server.MapPath(Constants.ItemImagePathBase);
            // Ensure directory exists
            Directory.CreateDirectory(imagesDir);

            // Build a unique filename by appending a counter if necessary
            var candidateFileName = baseFileName + extension;
            var candidatePath = Path.Combine(imagesDir, candidateFileName);
            var counter = 1;
            while (File.Exists(candidatePath))
            {
                candidateFileName = $"{baseFileName}_{counter}{extension}";
                candidatePath = Path.Combine(imagesDir, candidateFileName);
                counter++;
            }

            file.SaveAs(candidatePath);

            // Return the web-relative path
            return Constants.ItemImagePathBase.TrimEnd('/') + "/" + candidateFileName;
        }
    }
}