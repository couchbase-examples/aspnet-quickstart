using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
