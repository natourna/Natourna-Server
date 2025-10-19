using Microsoft.AspNetCore.Mvc;

namespace BuildingManagement.Controllers
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
                service = "BuildingManagement API"
            });
        }
    }
}
