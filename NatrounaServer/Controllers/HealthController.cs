using Microsoft.AspNetCore.Mvc;

namespace NatrounaServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "NatrounaServer API"
            });
        }
    }
}
