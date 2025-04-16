using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using galaxy_match_make.Services;

namespace galaxy_match_make.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly GoogleAuthService _authService;

        public AuthController(GoogleAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            try
            {
                AuthResponse authData = await _authService.ExchangeCodeForToken(code);

                return Ok(authData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Authentication failed: {ex.Message}");
            }
        }
    }
}