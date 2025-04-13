using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileController(IProfileRepository profileRepository) => _profileRepository = profileRepository;

        [HttpGet]
        public async Task<ActionResult<List<ProfileDto>>> GetAllProfiles()
        {
            var Profiles = await _profileRepository.GetAllProfiles();
            return Ok(Profiles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileDto>> GetProfileById(Guid id)
        {
            var Profile = await _profileRepository.GetProfileById(id);
            if (Profile == null)
            {
                return NotFound("This profile does not exist");
            }
            return Ok(Profile);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProfileDto>> UpdateProfile(Guid id, [FromBody] UpdateProfileDto profile)
        {
            if (profile == null)
            {
                return BadRequest("Profile data is required.");
            }

            var updatedProfile = await _profileRepository.UpdateProfile(id, profile);
            return Ok(updatedProfile);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileDto>> CreateProfile([FromBody] CreateProfileDto profile)
        {
            if (profile == null)
            {
                return BadRequest("Profile data is required.");
            }

            try
            {
                var createdProfile = await _profileRepository.CreateProfile(profile);
                return CreatedAtAction(nameof(GetProfileById), new { id = profile.UserId }, createdProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating profile: {ex.Message}");
            }
        }
    }
}
