using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace NatournaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "Version not found";
            return Ok(version);
        }
    }
}
