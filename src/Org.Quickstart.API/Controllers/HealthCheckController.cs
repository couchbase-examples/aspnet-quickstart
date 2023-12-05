using System;
using Microsoft.AspNetCore.Mvc;

namespace Org.Quickstart.API.Controllers
{
    [ApiController]
    [Route("healthcheck")]
    public class HealthCheckController : ControllerBase
    {

        [HttpGet]
        public string Index()
        {
            return $"currentDate: {DateTime.UtcNow}";
        }
    }
}
