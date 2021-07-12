using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engie.PCC.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;
                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    if (env.IsDevelopment())
                    {
                        try
                        {
                            config.AddUserSecrets<Startup>();
                        }
                        catch
                        {
                            //Do nothing
                        }
                    }
                })
                .UseKestrel(c => c.AddServerHeader = false)
                .UseIIS()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureLogging(((hostingContext, logBuilder) =>
                {
                    logBuilder.ClearProviders();

                    logBuilder.AddDebug();
                    logBuilder.AddConsole();
                }));
    }
}
