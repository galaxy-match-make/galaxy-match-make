using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacteristicCategoriesController(IGenericRepository<CharacteristicCategoriesDto> characteristicCategoriesRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacteristicCategoriesDto>>> GetAllCharacteristicCategories()
        {
            return Ok(await characteristicCategoriesRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacteristicCategoriesDto>> GetCharacteristicCategoriesById(int id)
        {
            var characteristicCategories = await characteristicCategoriesRepository.GetByIdAsync(id);

            return characteristicCategories is null
                ? BadRequest($"Characteristic with id {id} does not exist")
                : Ok(characteristicCategories);
        }

        [HttpPost]
        public async Task<ActionResult<CharacteristicCategoriesDto>> CreateCharacteristicCategories(CharacteristicCategoriesDto characteristicCategories)
        {
            characteristicCategories.Id = await characteristicCategoriesRepository.CreateAsync(characteristicCategories);
            
            return Created("CharacteristicCategories", characteristicCategories);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CharacteristicCategoriesDto>> UpdateCharacteristicCategories(int id, CharacteristicCategoriesDto characteristicCategories)
        {
            return await characteristicCategoriesRepository.GetByIdAsync(id) is null
                 ? BadRequest($"Characteristic with id {id} does not exist")
                 : Ok(await characteristicCategoriesRepository.UpdateAsync(characteristicCategories));
        }
    }
}
