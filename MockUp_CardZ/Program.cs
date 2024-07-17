using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using MockUp_CardZ;

namespace MockUp_CardZ
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:9991"); // Change the port to 5000
    }
}
