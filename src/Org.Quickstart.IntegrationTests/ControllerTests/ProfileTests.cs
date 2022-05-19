using Newtonsoft.Json;

using Org.Quickstart.API;
using Org.Quickstart.API.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Org.Quickstart.IntegrationTests.ControllerTests
{
    public class ProfileTests
        : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
		private readonly string baseHostname = "/api/v1/profile";
		private readonly string baseHostnameSearch = "/api/v1/profiles";
		private readonly string baseHostnameTransfer = "/api/v1/transfer";

        public ProfileTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task InsertProfileTestAsync()
        {
	        //create user
            var userProfile = GetProfile();
            var newUser = JsonConvert.SerializeObject(userProfile);
            var content = new StringContent(newUser, Encoding.UTF8, "application/json");
	        var response = await _client.PostAsync(baseHostname, content);

	        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
	        var jsonResults = await response.Content.ReadAsStringAsync();
	        var newUserResult = JsonConvert.DeserializeObject<Profile>(jsonResults);
            
	        //validate creation 
	        Assert.Equal(userProfile.FirstName, newUserResult.FirstName);  
	        Assert.Equal(userProfile.LastName, newUserResult.LastName);  
	        Assert.Equal(userProfile.Email, newUserResult.Email);  

	        //remove user
	        var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}"); 
	        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateProfileTestAsync()
        {
            //create user
            var userProfile = GetProfile();
            var newUser = JsonConvert.SerializeObject(userProfile);
            var content = new StringContent(newUser, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(baseHostname, content);

            var jsonResults = await response.Content.ReadAsStringAsync();
            var newUserResult = JsonConvert.DeserializeObject<Profile>(jsonResults);
            
	        //update user
	        UpdateProfile(newUserResult);            
            
	        var updateUserJson = JsonConvert.SerializeObject(newUserResult);
	        var updateContent =  new StringContent(updateUserJson, Encoding.UTF8, "application/json");
	        var updateResponse = await _client.PutAsync($"{baseHostname}/", updateContent);
            
	        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
	        var jsonResult = await updateResponse.Content.ReadAsStringAsync();    
            var updateUserResult = JsonConvert.DeserializeObject<Profile>(updateUserJson);

	        //validate update worked
	        Assert.Equal(newUserResult.Email, updateUserResult.Email);
	        Assert.Equal(newUserResult.FirstName, updateUserResult.FirstName);
	        Assert.Equal(newUserResult.LastName, updateUserResult.LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{updateUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

		}

        [Fact]
        public async Task GetProfileTestAsync()
        {
			//create user
			var userProfile = GetProfile();
			var newUser = JsonConvert.SerializeObject(userProfile);
			var content = new StringContent(newUser, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(baseHostname, content);

			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var jsonResults = await response.Content.ReadAsStringAsync();
			var newUserResult = JsonConvert.DeserializeObject<Profile>(jsonResults);
			
			//get the user from the main API
			var getResponse = await _client.GetAsync($"{baseHostname}/{newUserResult.Pid}"); 
			Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
			var getJsonResult = await getResponse.Content.ReadAsStringAsync();
			var getUserResult = JsonConvert.DeserializeObject<Profile>(getJsonResult);

			//validate it got the same user
			Assert.Equal(newUserResult.Email, getUserResult.Email);
	        Assert.Equal(newUserResult.FirstName, getUserResult.FirstName);
	        Assert.Equal(newUserResult.LastName, getUserResult.LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
		}

		[Fact]
		public async Task GetProfileSearchTestAsync()
		{
			await Task.Delay(10000);
			//create user
			var userProfile = GetProfile();
			var newUser = JsonConvert.SerializeObject(userProfile);
			var content = new StringContent(newUser, Encoding.UTF8, "application/json");
			var response = await _client.PostAsync(baseHostname, content);

			Assert.Equal(HttpStatusCode.Created, response.StatusCode);
			var jsonResults = await response.Content.ReadAsStringAsync();
			var newUserResult = JsonConvert.DeserializeObject<Profile>(jsonResults);

			//get the user from the main API
			var getResponse = await _client.GetAsync($"{baseHostnameSearch}?Search={userProfile.FirstName}&Skip=0&Limit=5");
			Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
			var getJsonResult = await getResponse.Content.ReadAsStringAsync();
			var getUserResult = JsonConvert.DeserializeObject<List<Profile>>(getJsonResult);

			//validate it got the same user
			Assert.Equal(newUserResult.Email, getUserResult[0].Email);
			Assert.Equal(newUserResult.FirstName, getUserResult[0].FirstName);
			Assert.Equal(newUserResult.LastName, getUserResult[0].LastName);

			//remove user
			var deleteResponse = await _client.DeleteAsync($"{baseHostname}/{newUserResult.Pid}");
			Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
		}

        [Fact]
        public async Task TransferOnBoardCreditAsync()
        {
            // amount to transfer
            var transferAmount = 50.0M;

            //create users
            var fromProfile = GetProfile();
            var fromUser = JsonConvert.SerializeObject(fromProfile);
            var fromContent = new StringContent(fromUser, Encoding.UTF8, "application/json");
            var fromResponse = await _client.PostAsync(baseHostname, fromContent);
            var fromJson = await fromResponse.Content.ReadAsStringAsync();
            fromProfile = JsonConvert.DeserializeObject<Profile>(fromJson);

            var toProfile = GetProfile();
            var toUser = JsonConvert.SerializeObject(toProfile);
            var toContent = new StringContent(toUser, Encoding.UTF8, "application/json");
            var toResponse = await _client.PostAsync(baseHostname, toContent);
            var toJson = await toResponse.Content.ReadAsStringAsync();
            toProfile = JsonConvert.DeserializeObject<Profile>(toJson);

            // transfer funds
            var transfer = new ProfileTransferCredit
            {
                Pfrom = fromProfile.Pid,
                Pto = toProfile.Pid,
                Amount = transferAmount
            };
            var transferJson = JsonConvert.SerializeObject(transfer);
            var transferContent = new StringContent(transferJson, Encoding.UTF8, "application/json");
            await _client.PostAsync(baseHostnameTransfer, transferContent);

            // validate "from" user amount is down
            var newFromResponse = await _client.GetAsync($"{baseHostname}/{fromProfile.Pid}");
            var newFromJsonResult = await newFromResponse.Content.ReadAsStringAsync();
            var newFromProfile = JsonConvert.DeserializeObject<Profile>(newFromJsonResult);
            Assert.Equal(newFromProfile.OnBoardCredit, fromProfile.OnBoardCredit - transferAmount);

            // validate "to" user amount is up
            var newToResponse = await _client.GetAsync($"{baseHostname}/{toProfile.Pid}");
            var newToJsonResult = await newToResponse.Content.ReadAsStringAsync();
            var newToProfile = JsonConvert.DeserializeObject<Profile>(newToJsonResult);
            Assert.Equal(newToProfile.OnBoardCredit, toProfile.OnBoardCredit + transferAmount);
        }

        private Profile GetProfile()
	    {
	        return new ProfileCreateRequestCommand(){
		        FirstName = "John",
		        LastName = "Doe",
		        Email = "john.doe@couchbase.com",
		        Password = "password",
				OnBoardCredit = 500
		    }.GetProfile(); 
	    }

	    private void UpdateProfile (Profile profile)
	    {
	        profile.FirstName = "Jane";
	        profile.LastName = "Smith";
	        profile.Email = "Jane.Smith@couchbase.com";
	        profile.Password = "password1";
            profile.OnBoardCredit = profile.OnBoardCredit;
        }
    }
}
