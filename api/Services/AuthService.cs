using System.Security.Claims;
using System.Text.Json.Serialization;
using galaxy_match_make.Models;
using galaxy_match_make.Repositories;
using Google.Apis.Auth;


namespace galaxy_match_make.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IUserRepository _userRepository;
        private readonly IProfileRepository _profileRepository;

        public GoogleAuthService(IConfiguration config, HttpClient httpClient, IUserRepository userRepository, IProfileRepository profileRepository)
        {
            _config = config;
            _httpClient = httpClient;
            _userRepository = userRepository;
            _profileRepository = profileRepository;
        }

        public async Task<AuthResponse> ExchangeCodeForToken(string code)
        {
            var tokenRequest = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", _config["Google:ClientId"]},
                {"client_secret", _config["Google:ClientSecret"]},
                {"redirect_uri", _config["Google:RedirectUri"]},
                {"grant_type", "authorization_code"}
            };
            
            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));
            
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();

            
            GoogleJsonWebSignature.Payload payload = await ValidateGoogleToken(tokenResponse.IdToken);
            
            UserDto userDto = await _userRepository.GetUserByOauthId(payload.Subject);
            Boolean profileComplete = false;

            if (userDto == null)
            {
                await _userRepository.AddUser(payload.Subject);
            }
            else
            {
                ProfileDto profile = await _profileRepository.GetProfileById(userDto.Id);
                profileComplete = profile != null;
            }
           

            return new AuthResponse
            {
                UserId = userDto.Id,
                Email = payload.Email,
                Name = payload.Name,
                JwtToken = tokenResponse.IdToken,
                ProfileComplete = profileComplete
                
            };
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateGoogleToken(string idToken)
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
        }

        public string GoogleLogin()
        {

            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={_config["Google:ClientId"]}&" +
                   $"redirect_uri={_config["Google:RedirectUri"]}&" +
                   $"response_type=code&" +
                   $"scope=openid%20email%20profile&";
        }
        
        public async Task<ClaimsPrincipal> ValidateGoogleJwtAndCreatePrincipal(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            });

            var user = await _userRepository.GetUserByOauthId(payload.Subject);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }


    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
    
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string JwtToken { get; set; }
        public bool ProfileComplete { get; set; }
    }
}
