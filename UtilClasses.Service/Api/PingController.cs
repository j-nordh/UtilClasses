using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MACS.Service.Api
{
    [Controller]
    [Route("Ping")]
    public class PingController
    {
        [HttpGet]
        [Route("")]
        [SwaggerOperation("Pings the service", "To ensure that the service is live, call this route to receive a timestamp.")]
        public IActionResult Ping()=> new OkObjectResult(DateTime.UtcNow);
        
    }
}
