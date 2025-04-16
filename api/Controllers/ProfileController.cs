using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using galaxy_match_make.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Security.Claims;
 
namespace galaxy_match_make.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProfileService _profileService;
 
        public ProfileController(IProfileRepository profileRepository, IUserRepository userRepository, IProfileService profileService)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
            _profileService = profileService;
        }
 
        [HttpGet]
        public async Task<ActionResult<List<ProfileDto>>> GetAllProfiles()
        {
            var Profiles = await _profileRepository.GetAllProfiles();
            return Ok(Profiles);
        }
 
        [HttpGet("{id}/pending-likes")]
        public async Task<ActionResult<List<ProfileDto>>> GetPendingMatchesByUserId(Guid id)
        {
            var Profiles = await _profileRepository.GetPendingMatchesByUserId(id);
            if (Profiles == null)
            {
                return BadRequest("No pending likes found");
            }
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
 
        [Authorize]
        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileDto profile)
        {
            if (profile == null)
            {
                return BadRequest("Profile data is required.");
            }
 
            var profileClaimToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
 
            if (string.IsNullOrEmpty(profileClaimToken) || !Guid.TryParse(profileClaimToken, out Guid userId))
            {
                return Unauthorized("User ID not found in token or invalid format");
            }
 
            var updatedProfile = await _profileRepository.UpdateProfile(userId, profile);
            return Ok(updatedProfile);
        }
 
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProfileDto>> CreateProfile([FromBody] CreateProfileDto profile)
        {
            if (profile == null)
            {
                return BadRequest("Profile data is required.");
            }
            
            var profileClaimToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(profileClaimToken) || !Guid.TryParse(profileClaimToken, out Guid userId))
            {
                return Unauthorized("User ID not found in token or invalid format");
            }

            ProfileDto profileDto = new ProfileDto()
            {
                UserId = userId,
                DisplayName = profile.DisplayName,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl
            };

            profileDto.Id = await _profileService.CreateAsync(profileDto);
            
            return Created("Profile", profileDto);
            
            // SAVE USER INTERESTS
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ProfileDto>> GetMyProfile()
        {
            var profileClaimToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(profileClaimToken) || !Guid.TryParse(profileClaimToken, out Guid userId))
            {
                return Unauthorized("User ID not found in token or invalid format");
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound($"Reactor user with ID {userId} not found");
            }

            var profile = await _profileRepository.GetProfileById(userId);
            if (profile == null)
            {
                return NotFound("Profile not found");
            }

            return Ok(profile);
        }

        [HttpGet("{profileId}/preferred_profiles")]
        public async Task<ActionResult<IEnumerable<ProfileDto>>> GetPreferredProfiles(int profileId)
        {
            return Ok(await _profileService.GetPreferredProfiles(profileId));
        }

        [Authorize]
        [HttpGet("matched")]
        public async Task<ActionResult<List<MatchedProfileDto>>> GetUserMatchedProfilesFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("You are not authorized to access this resource.");
            }
            else
            {
                var matchedProfiles = await _profileRepository.GetUserMatchedProfiles(userId);
 
                if (matchedProfiles == null)
                {
                    return NotFound("No matched profiles found.");
                }
                else
                {
                    return Ok(matchedProfiles);
                }
            }
        }
    }
}