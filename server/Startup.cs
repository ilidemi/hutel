using System;
using hutel.Filters;
using hutel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hutel
{
    public class Startup
    {
        private IWebHostEnvironment _environment;

        public Startup(IWebHostEnvironment env)
        {
            this._environment = env;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.DateParseHandling = DateParseHandling.None;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.Formatting = Formatting.Indented;
                    opt.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                });
            services.AddScoped<ValidateModelStateAttribute>();
            services.AddLogging(opt => opt.AddConsole());

            if (this._environment.IsProduction())
            {
                services.AddLettuceEncrypt();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRedirectToHttpsMiddleware();
            app.UseStaticFiles();
            if (Environment.GetEnvironmentVariable("HUTEL_USE_BASIC_AUTH") == "1")
            {
                app.UseBasicAuthMiddleware();
            }
            if (Environment.GetEnvironmentVariable("HUTEL_USE_SINGLE_USER_AUTH") == "1")
            {
                app.UseSingleUserAuthMiddleware();
            }
            if (Environment.GetEnvironmentVariable("HUTEL_USE_GOOGLE_AUTH") == "1")
            {
                app.UseGoogleAuthMiddleware();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
