using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Engie.PCC.Api.Tests
{
    public class HttpServerFixture<TEngieApiStartup> : IDisposable
    {
        public const int EngieApiPort = 44352;
        public const string EngieApiUri = "http://localhost:44352";
        private const string EngieNotificationUri = "ws://localhost:44352/notifications";

        private const string SolutionName = "Engie.PCC.Api.sln";
        private IWebHost _apiServer;
        private bool disposed;

        public HttpServerFixture() : this("src")
        {
        }

        protected HttpServerFixture(string solutionRelativeTargetProjectParentDir)
        {
            CreateApiServer(solutionRelativeTargetProjectParentDir);

            EngieApiClient = new HttpClient();
            EngieApiClient.BaseAddress = new Uri(EngieApiUri);

            WebSocketClient = new ApiClient(new Uri(EngieNotificationUri));
        }

        ~HttpServerFixture() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

            disposed = true;
            if (!disposing) { return; }

            WebSocketClient?.Dispose();
            EngieApiClient?.Dispose();

            _apiServer?.StopAsync().Wait();
            _apiServer?.Dispose();
        }

        public HttpClient EngieApiClient { get; private set; }
        public ApiClient WebSocketClient { get; private set; }


        private void CreateApiServer(string solutionRelativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TEngieApiStartup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath(solutionRelativeTargetProjectParentDir, startupAssembly);

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .UseEnvironment("IntegrationTests")
                .UseStartup(typeof(TEngieApiStartup))
                .UseKestrel(options => options.Listen(IPAddress.Loopback, EngieApiPort))
                .UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile(configPath)
                    .Build()
                )
                //.ConfigureServices(OnConfigureApiServices)
                .ConfigureLogging((hostingContext, logBuilder) =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.AddDebug();
                    logBuilder.AddConsole();
                });

            _apiServer = builder.Build();
            _apiServer.Start();
        }

        //private void OnConfigureApiServices(IServiceCollection services)
        //{
        //    services.AddLogging();
        //    services.AddMvc(options => options.EnableEndpointRouting = true)
        //        .SetCompatibilityVersion(CompatibilityVersion.Latest)
        //        .AddApplicationPart(typeof(TEngieApiStartup).Assembly);
        //}

        /// <summary>
        /// Gets the full path to the target project path that we wish to test
        /// </summary>
        /// <param name="solutionRelativePath">
        /// The parent directory of the target project.
        /// e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        private static string GetProjectPath(string solutionRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            //var projectFolder = startupAssembly.GetName().Name;
            var projectFolder = Path.GetDirectoryName(startupAssembly.Location);

            // Get currently executing test project path
            var applicationBasePath = System.AppDomain.CurrentDomain.BaseDirectory;

            // Find the folder which contains the solution file. We then use this information to find the target
            // project which we want to test.
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, SolutionName));
                if (solutionFileInfo.Exists)
                {
                    return Path.GetFullPath(Path.Combine(directoryInfo.FullName, solutionRelativePath, projectFolder));
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Solution root could not be located using application root {applicationBasePath}.");
        }
    }
}
