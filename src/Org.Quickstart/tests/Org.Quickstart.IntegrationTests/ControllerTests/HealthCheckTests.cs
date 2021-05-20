using Org.Couchbase.Quickstart.API;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Org.Quickstart.IntegrationTests.ControllerTests
{
    public class HealthCheckTests
        : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public HealthCheckTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ReturnHealthCheck()
        {
            var response = await _client.GetAsync("/healthcheck");
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();

            Assert.Contains("currentDate", stringResponse);
        }
    }
}
