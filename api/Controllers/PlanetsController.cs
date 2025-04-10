using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanetsController : ControllerBase
    {
        private readonly IPlanetRepository _planetRepository;
        public PlanetsController(IPlanetRepository planetRepository) => _planetRepository = planetRepository;

        [HttpGet]
        public async Task<ActionResult<PlanetDto>> GetAllPlanets()
        {
            var planets = await _planetRepository.GetAllPlanetsAsync();
            return Ok(planets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanetDto>> GetPlanetById(int id)
        {
            var planet = await _planetRepository.GetPlanetByIdAsync(id);

            if (planet == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(planet);
            }
        }
    }
}
