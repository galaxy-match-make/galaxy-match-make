using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> CheckHealth()
        {
            return Ok("I'm healthy");
        }
    }
}
