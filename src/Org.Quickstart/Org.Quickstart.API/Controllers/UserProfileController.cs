using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Quickstart.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Org.Quickstart.API.Controllers
{
    [ApiController]
    [Route("userprofiles")]
    public class UserProfileController
        : Controller
    {
        private readonly IClusterProvider _clusterProvider;
        private readonly IBucketProvider _bucketProvider;
        private readonly ILogger _logger;

        private readonly CouchbaseConfig _couchbaseConfig;

        public UserProfileController(
            IClusterProvider clusterProvider,
            IBucketProvider bucketProvider,
	        IOptions<CouchbaseConfig> options,
            ILogger<ProfilesController> logger)
        {
	        _clusterProvider = clusterProvider;
	        _bucketProvider = bucketProvider;
            _logger = logger;

	        _couchbaseConfig = options.Value;
        }

        [HttpGet("{id:Guid}", Name = "UserProfile-GetById")]
        [SwaggerOperation(OperationId = "UserProfile-GetById", Summary = "Get user profile by Id", Description = "Get a user profile by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);

		        var scope = bucket.Scope(_couchbaseConfig.ScopeName);
                var collection = scope.Collection(_couchbaseConfig.CollectionName); 
		        var result = await collection.GetAsync(id.ToString());
                return Ok(result.ContentAs<Profile>());

            }
	        catch (DocumentNotFoundException)
	        {
		        return NotFound();
		    }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "UserProfile-List", Summary = "Get user profiles", Description = "Get a list of user profiles from the request")]
        [SwaggerResponse(200, "Returns the list of user profiles")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> List([FromQuery] ProfileRequest request)
        {
            try
            {
		        var cluster = await _clusterProvider.GetClusterAsync();
		        var query = $"SELECT p.* FROM user_profile._default.profile p WHERE lower(p.firstName) LIKE '%{request.FirstNameSearch.ToLower()}%' LIMIT {request.Limit} OFFSET {request.Skip}"; 

	            var results = await cluster.QueryAsync<Profile>(query);
		        var items = await results.Rows.ToListAsync<Profile>();
		        if (items.Count == 0)
		            return NotFound();

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPost]
        [SwaggerOperation(OperationId = "UserProfile-Post", Summary = "Create a user profile", Description = "Create a user profile from the request")]
        [SwaggerResponse(201, "Create a user profile")]
        [SwaggerResponse(409, "the email of the user already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromBody] ProfileCreateRequestCommand request)
        {
            try
            {
		        var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		        var profile = request.GetProfile();
		        profile.Id = Guid.NewGuid();
		        await collection.InsertAsync(profile.Id.ToString(), profile);

                return Ok(profile);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPut]
        [SwaggerOperation(OperationId = "UserProfile-Update", Summary = "Update a user profile", Description = "Update a user profile from the request")]
        [SwaggerResponse(200, "Update a user profile")]
        [SwaggerResponse(404, "user profile not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Update([FromBody] ProfileUpdateRequestCommand request)
        {
            try
            {
                var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
                var collection = bucket.Collection(_couchbaseConfig.CollectionName);
                var result = await collection.GetAsync(request.Id.ToString());
                var profile = result.ContentAs<Profile>();
		
                var updateResult = await collection.ReplaceAsync<Profile>(request.Id.ToString(), request.GetProfile());
                return Ok(profile);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }


        [HttpDelete("{id:Guid}")]
        [SwaggerOperation(OperationId = "UserProfile-Delete", Summary = "Delete a profile", Description = "Delete a profile from the request")]
        [SwaggerResponse(200, "Delete a profile")]
        [SwaggerResponse(404, "profile not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
		        var bucket = await _bucketProvider.GetBucketAsync(_couchbaseConfig.BucketName);
		        var collection = bucket.Collection(_couchbaseConfig.CollectionName);
		        await collection.RemoveAsync(id.ToString());
                return this.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
