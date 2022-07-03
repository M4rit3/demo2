using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using api.Image;


[assembly: FunctionsStartup(typeof(Startup))]
namespace api.Image
{

    public class Startup : FunctionsStartup
    {
          public override void Configure(IFunctionsHostBuilder build)

          {
            var config = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                         .AddEnvironmentVariables()
                         .Build();

                build.Services.AddSingleton<IConfiguration>(config);
                build.Services.AddTransient<IImageRepository, ImageRepository>();         
          }
    }


}