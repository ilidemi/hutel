using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace hutel
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

                    if (isProduction)
                    {
                        webBuilder.UseKestrel(kestrelOptions =>
                        {
                            kestrelOptions.ListenAnyIP(80);
                            kestrelOptions.ListenAnyIP(443, listenOptions =>
                            {
                                listenOptions.UseHttps(h =>
                                {
                                    h.UseLettuceEncrypt(kestrelOptions.ApplicationServices);
                                });
                            });
                        });
                    }
                });
    }
}
