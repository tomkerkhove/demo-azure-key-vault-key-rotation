using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace TomKerkhove.Demos.KeyVault.API
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
        }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
    }
}