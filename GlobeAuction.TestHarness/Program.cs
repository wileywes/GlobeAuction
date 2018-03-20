using System;
using System.IO;
using Tesseract;

namespace GlobeAuction.TestHarness
{
    class Program
    {
        private static string BidSheetsFolder = @"D:\Development\GlobeAuction\Content\Images\BidSheets";

        static void Main(string[] args)
        {
            SingleItem();
        }

        private static void SingleItem()
        {
            const string tessDataDir = @"D:\Development\GlobeAuction.UnitTests";

            var filePath = Path.Combine(BidSheetsFolder, "SingleWinner.JPG");

            using (var engine = new TesseractEngine(tessDataDir, "eng", EngineMode.Default))
            {
                using (var image = Pix.LoadFromFile(filePath))
                {
                    using (var page = engine.Process(image))
                    {
                        string text = page.GetText();
                        Console.WriteLine(text);
                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
