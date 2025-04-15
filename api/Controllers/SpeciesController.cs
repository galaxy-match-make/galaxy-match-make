using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly ISpeciesRepository _speciesRepository;

    public SpeciesController(ISpeciesRepository speciesRepository)
    {
        _speciesRepository = speciesRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSpecies()
    {
        var species = await _speciesRepository.GetAllSpeciesAsync();
        return Ok(species);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpeciesById(int id)
    {
        var species = await _speciesRepository.GetSpeciesByIdAsync(id);
        if (species == null)
        {
            return NotFound();
        }
        return Ok(species);
    }
}