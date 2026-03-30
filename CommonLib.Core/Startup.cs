using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CommonLib.Core
{
    public class Startup
    {
        public static IConfigurationSection? Properties { get; private set; }
        public Startup(IConfiguration configuration, IServiceProvider? serviceProvider = null)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
            Properties = Configuration.GetSection("CommonLib.Core");
        }

        public IConfiguration Configuration { get; }
        public static IServiceProvider? ServiceProvider { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
