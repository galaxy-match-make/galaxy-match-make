using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileAttributesController(IGenericRepository<ProfileAttributesDto> profileAttributesRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfileAttributesDto>>> GetAllProfileAttributes()
        {
            var profileAttributes = await profileAttributesRepository.GetAllAsync();
            return Ok(profileAttributes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileAttributesDto>> GetProfileAttributesById(int id)
        {
            var profileAttributes = await profileAttributesRepository.GetByIdAsync(id);

            return profileAttributes is null
                ? BadRequest($"Characteristic with id {id} does not exist")
                : Ok(profileAttributes);
        }
        
        [HttpGet("profile/{profileId}")]
        public async Task<ActionResult<IEnumerable<ProfileAttributesDto>>> GetProfileAttributesByProfile(int profileId)
        {
            var profileAttributes = await profileAttributesRepository.GetByColumnValueAsync("profile_id", profileId);
            return Ok(profileAttributes);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileAttributesDto>> CreateProfileAttributes(ProfileAttributesDto profileAttributes)
        {
            profileAttributes.Id = await profileAttributesRepository.CreateAsync(profileAttributes);
            
            return Created("ProfileAttributes", profileAttributes);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProfileAttributesDto>> UpdateProfileAttributes(int id, ProfileAttributesDto profileAttributes)
        {
            return await profileAttributesRepository.GetByIdAsync(id) is null
                 ? BadRequest($"Characteristic with id {id} does not exist")
                 : Ok(await profileAttributesRepository.UpdateAsync(profileAttributes));
        }
    }
}
