using galaxy_match_make.Models;
using galaxy_match_make.Services;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanetsController : ControllerBase
    {
        private readonly IPlanetService _planetService;
        public PlanetsController(IPlanetService service) => _planetService = service;

        [HttpGet]
        public async Task<ActionResult<PlanetDto>> GetAllPlanets()
        {
            var planets = await _planetService.GetAllPlanetsAsync();
            return Ok(planets);
        }
    }
}
