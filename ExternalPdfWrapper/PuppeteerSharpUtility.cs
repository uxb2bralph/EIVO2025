using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalPdfWrapper
{
    internal class PuppeteerSharpUtility : IDisposable
    {
        private IBrowser _browser;
        private static readonly SemaphoreSlim _launchLock = new SemaphoreSlim(1, 1);

        PdfOptions pdfOptions = new PdfOptions
        {
            PrintBackground = true,
            DisplayHeaderFooter = false,
            Landscape = false,
            PreferCSSPageSize = true,
            MarginOptions = new MarginOptions
            {
                Top = "0",
                Bottom = "0",
                Left = "0",
                Right = "0"
            }
        };

        // navigation options are created per-call to avoid shared mutable state across threads

        public async Task<int> UsePuppeteerSharp(string url, string outputFile, double timeOutInMinute)
        {
            try
            {
                // Ensure browser is launched only once. Multiple threads will await the semaphore
                if (_browser == null)
                {
                    await _launchLock.WaitAsync();
                    try
                    {
                        if (_browser == null)
                        {
                            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                            {
                                Headless = true,
                                ExecutablePath = AppSettings.Default.Command,
                                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage" }
                            });
                        }
                    }
                    finally
                    {
                        _launchLock.Release();
                    }
                }

                // Create a page per-call; Puppeteer supports multiple pages concurrently
                using var page = await _browser!.NewPageAsync();
                Console.WriteLine("Navigating to: " + url);

                var navigationOptionsLocal = new NavigationOptions
                {
                    Timeout = (int)(timeOutInMinute * 60000),
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                };

                await page.GoToAsync(url, navigationOptionsLocal);

                Console.WriteLine($"Generating PDF -> {outputFile}");

                var pdfOptionsLocal = new PdfOptions
                {
                    PrintBackground = pdfOptions.PrintBackground,
                    DisplayHeaderFooter = pdfOptions.DisplayHeaderFooter,
                    Landscape = pdfOptions.Landscape,
                    PreferCSSPageSize = pdfOptions.PreferCSSPageSize,
                    MarginOptions = new MarginOptions
                    {
                        Top = pdfOptions.MarginOptions.Top,
                        Bottom = pdfOptions.MarginOptions.Bottom,
                        Left = pdfOptions.MarginOptions.Left,
                        Right = pdfOptions.MarginOptions.Right
                    }
                };

                await page.PdfAsync(outputFile, pdfOptionsLocal);

                Console.WriteLine("PDF generated successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex);
                return 1;
            }
        }

        public void Dispose()
        {
            if (_browser != null)
            {
                _browser.Dispose();
                _browser = null;
            }
        }
        public static async Task UsePuppeteerSharpConvertHtmlToPDF(string htmlSource, string pdfFile, double timeOutInMinute, string[] args)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = AppSettings.Default.Command,
                Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            await page.GoToAsync(htmlSource, new NavigationOptions
            {
                Timeout = (int)(timeOutInMinute * 60000),
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                PrintBackground = true,
                DisplayHeaderFooter = false,
                Landscape = false,
                PreferCSSPageSize = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Bottom = "0",
                    Left = "0",
                    Right = "0"
                }
            };

            //var pdfBytes = await page.PdfDataAsync(pdfOptions);
            //File.WriteAllBytes(pdfFile, pdfBytes);
            await page.PdfAsync(pdfFile, pdfOptions);
        }
    }
}
