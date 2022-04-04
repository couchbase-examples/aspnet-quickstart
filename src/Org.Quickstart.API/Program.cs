using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Org.Quickstart.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(opts =>
                    {
                        // Bind directly to a socket handle or Unix socket
                        opts.ListenAnyIP(5001);
                    });
                    webBuilder.UseUrls("http://0.0.0.0:5001");
                });
    }
}
