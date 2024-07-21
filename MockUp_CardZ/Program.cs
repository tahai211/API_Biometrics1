using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace MockUp_CardZ
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Thiết lập cấu hình cho ứng dụng
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                        .Build();

                    // Sử dụng cấu hình này
                    webBuilder.UseConfiguration(configuration);

                    // Sử dụng lớp Startup để cấu hình dịch vụ và pipeline
                    webBuilder.UseStartup<Startup>();

                    // Cấu hình Kestrel để lắng nghe trên các cổng HTTP và HTTPS
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenAnyIP(9991, listenOptions =>
                        {
                            listenOptions.UseHttps(); // Sử dụng HTTPS trên cổng 9990
                        });
                    });
                });
    }
}
