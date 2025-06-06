using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace WebHome
{
    public class Program
    {
        public static void Main(string[]? args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddProvider(new FileLoggerProvider());
                    logging.AddDebug();
                    //logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddControllers(options =>
                    {
                        options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                        options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
                    })
                    .AddXmlSerializerFormatters()
                    .AddXmlDataContractSerializerFormatters();
                });
    }
}
