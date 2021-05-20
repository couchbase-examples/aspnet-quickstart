using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Org.Quickstart.API;
using Org.Quickstart.API.Services;

namespace Org.Quickstart.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var hbuilder = base.CreateHostBuilder();
            return hbuilder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
        }
	}
}
