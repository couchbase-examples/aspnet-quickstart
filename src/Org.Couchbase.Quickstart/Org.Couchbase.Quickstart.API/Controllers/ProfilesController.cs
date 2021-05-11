using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.Couchbase.Quickstart.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Org.Couchbase.Quickstart.API.Controllers
{
    [ApiController]
    [Route("profiles")]
    public class ProfilesController
        : Controller
    {
        /// <summary>
        /// logger
        /// </summary>
        protected readonly ILogger _logger;

        public ProfilesController(ILogger<ProfilesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id:Guid}", Name = "Profile-GetById")]
        [SwaggerOperation(OperationId = "Profile-GetById", Summary = "Get user profile by Id", Description = "Get a user profile by Id from the request")]
        [SwaggerResponse(200, "Returns a report")]
        [SwaggerResponse(404, "Report not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> GetById([FromQuery] Guid id)
        {
            try
            {
                //todo add code in here and replace this part
                await Task.Delay(100);
                return Ok(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "Profile-List", Summary = "Get user profiles", Description = "Get a list of user profiles from the request")]
        [SwaggerResponse(200, "Returns the list of user profiles")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> List([FromQuery] ProfileRequest request)
        {
            try
            {
                //todo add code in here and replace this part
                await Task.Delay(100);
                return Ok(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPost]
        [SwaggerOperation(OperationId = "Profile-Post", Summary = "Create a user profile", Description = "Create a user profile from the request")]
        [SwaggerResponse(201, "Create a user profile")]
        [SwaggerResponse(409, "the email of the user already exists")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Post([FromBody] ProfileRequestCommand request)
        {
            try
            {
                //todo add code in here and replace this part
                await Task.Delay(100);
                return Ok(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }

        [HttpPut]
        [SwaggerOperation(OperationId = "Profile-Update", Summary = "Update a user profile", Description = "Update a user profile from the request")]
        [SwaggerResponse(200, "Update a user profile")]
        [SwaggerResponse(404, "user profile not found")]
        [SwaggerResponse(500, "Returns an internal error")]
        public async Task<IActionResult> Update([FromBody] ProfileRequestCommand request)
        {
            try
            {
                //todo add code in here and replace this part
                await Task.Delay(100);
                return Ok(string.Empty);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message} {ex.StackTrace} {Request.GetDisplayUrl()}");
            }
        }
    }
}
