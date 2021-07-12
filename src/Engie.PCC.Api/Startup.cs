using Engie.PCC.Api.Services;
using Engie.PCC.Api.Services.Calculators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Engie.PCC.Api.Middlewares;
using Engie.PCC.Api.Services.Hosted;
using Engie.PCC.Api.Notifiers;

namespace Engie.PCC.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBroadcastService, BroadcastService>();

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue>(ctx => {return new BackgroundTaskQueue(100);});

            services.AddScoped<PowerplantCalculatorFactory>();
            services.AddScoped<ILoadResultNotifier, LoadResultNotifier>();
            services.AddScoped<IProductionPlanService, ProductionPlanService>();

            services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Formatting.None;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                options.SerializerSettings.DateParseHandling = DateParseHandling.DateTime;
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            
            services.AddSwaggerGen(c =>
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Engie.PCC.Api.xml");
                c.IncludeXmlComments(xmlPath);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Engie.PCC.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Engie.PCC.Api v1"));
            }

            //app.UseHttpsRedirection();

            // Set up the web sockets functionality.
            app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();

            app.UseRouting();
            app.UseMiddleware(typeof(ApiExceptionHandlerMiddleware));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
