using System;
using System.IO;
using hutel.Filters;
using hutel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace server
{
    public class Startup
    {
        private const string _envUseBasicAuth = "HUTEL_USE_BASIC_AUTH";
        private const string _envUseGoogleAuth = "HUTEL_USE_GOOGLE_AUTH";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.DateParseHandling = DateParseHandling.None;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.Formatting = Formatting.Indented;
                    opt.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                });
            services.AddScoped<ValidateModelStateAttribute>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Trace);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRedirectToHttpsMiddleware();
            app.UseStaticFiles();
            if (Environment.GetEnvironmentVariable(_envUseBasicAuth) == "1")
            {
                app.UseBasicAuthMiddleware();
            }
            if (Environment.GetEnvironmentVariable(_envUseGoogleAuth) == "1")
            {
                app.UseGoogleAuthMiddleware();
            }
            app.UseMvc();
        }
    }
}
