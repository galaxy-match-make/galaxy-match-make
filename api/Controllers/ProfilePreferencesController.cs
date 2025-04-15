using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilePreferencesController(IGenericRepository<ProfilePreferencesDto> profilePreferencesRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfilePreferencesDto>>> GetAllProfilePreferences()
        {
            var profilePreferences = await profilePreferencesRepository.GetAllAsync();
            return Ok(profilePreferences);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfilePreferencesDto>> GetProfilePreferencesById(int id)
        {
            var profilePreferences = await profilePreferencesRepository.GetByIdAsync(id);

            return profilePreferences is null
                ? BadRequest($"Characteristic with id {id} does not exist")
                : Ok(profilePreferences);
        }
        
        [HttpGet("profile/{profileId}")]
        public async Task<ActionResult<IEnumerable<ProfilePreferencesDto>>> GetProfilePreferencesByProfile(int profileId)
        {
            var profilePreferences = await profilePreferencesRepository.GetByColumnValueAsync("profile_id", profileId);
            return Ok(profilePreferences);
        }

        [HttpPost]
        public async Task<ActionResult<ProfilePreferencesDto>> CreateProfilePreferences(ProfilePreferencesDto profilePreferences)
        {
            profilePreferences.Id = await profilePreferencesRepository.CreateAsync(profilePreferences);
            
            return Created("ProfilePreferences", profilePreferences);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProfilePreferencesDto>> UpdateProfilePreferences(int id, ProfilePreferencesDto profilePreferences)
        {
            return await profilePreferencesRepository.GetByIdAsync(id) is null
                 ? BadRequest($"Characteristic with id {id} does not exist")
                 : Ok(await profilePreferencesRepository.UpdateAsync(profilePreferences));
        }
    }
}
