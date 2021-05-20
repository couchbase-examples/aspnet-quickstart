using Org.Couchbase.Quickstart.API;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Org.Quickstart.IntegrationTests.ControllerTests
{
    public class ProfileTests
        : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public ProfileTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProfilesTestAsync()
        {
            //todo complete test
            await Task.Delay(100);
        }

        [Fact]
        public async Task GetProfileTestAsync()
        {
            //todo complete test
            await Task.Delay(100);
        }

        [Fact]
        public async Task UpdateProfileTestAsync()
        {
            //todo complete test
            await Task.Delay(100);
        }

        [Fact]
        public async Task InsertProfileTestAsync()
        {
            //todo complete test
            await Task.Delay(100);
        }
    }
}
