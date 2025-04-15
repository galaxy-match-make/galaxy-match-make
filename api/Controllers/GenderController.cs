using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenderController : ControllerBase
{
    private readonly IGenderRepository _genderRepository;

    public GenderController(IGenderRepository genderRepository)
    {
        _genderRepository = genderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGenders()
    {
        var genders = await _genderRepository.GetAllGendersAsync();
        return Ok(genders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenderById(int id)
    {
        var gender = await _genderRepository.GetGenderByIdAsync(id);
        if (gender == null)
        {
            return NotFound();
        }
        return Ok(gender);
    }
}