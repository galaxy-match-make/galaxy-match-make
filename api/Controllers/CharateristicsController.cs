using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacteristicsController(IGenericRepository<CharacteristicsDto> characteristicsRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacteristicsDto>>> GetAllCharacteristics()
        {
            var characteristics = await characteristicsRepository.GetAllAsync();
            return Ok(characteristics);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacteristicsDto>> GetCharacteristicsById(int id)
        {
            var characteristics = await characteristicsRepository.GetByIdAsync(id);

            return characteristics is null
                ? BadRequest($"Characteristic with id {id} does not exist")
                : Ok(characteristics);
        }

        [HttpPost]
        public async Task<ActionResult<CharacteristicsDto>> CreateCharacteristics(CharacteristicsDto characteristics)
        {
            characteristics.Id = await characteristicsRepository.CreateAsync(characteristics);
            
            return Created("Characteristics", characteristics);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CharacteristicsDto>> UpdateCharacteristics(int id, CharacteristicsDto characteristics)
        {
            return await characteristicsRepository.GetByIdAsync(id) is null
                 ? BadRequest($"Characteristic with id {id} does not exist")
                 : Ok(await characteristicsRepository.UpdateAsync(characteristics));
        }
    }
}
