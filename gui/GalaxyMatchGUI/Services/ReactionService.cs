using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.lib;

namespace GalaxyMatchGUI.Services
{
    public class ReactionService
    {
        private readonly HttpClient _httpClient;
        private string ApiBaseUrl = AppSettings.BackendUrl;

        public ReactionService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            _httpClient = new HttpClient(handler);
        }

        public async Task<bool> SendReactionAsync(Guid targetId, bool isPositive)
        {
            try
            {
                // Get the JWT token from storage
                var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
                
                if (string.IsNullOrEmpty(jwtToken))
                {
                    throw new InvalidOperationException("Authentication token is missing");
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var reactionRequest = new
                {
                    TargetId = targetId,
                    IsPositive = isPositive
                };

                var content = JsonContent.Create(reactionRequest);
                var response = await _httpClient.PostAsync($"{ApiBaseUrl}/api/Reactions", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to send reaction: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SendReactionAsync: {ex.Message}");
                return false;
            }
        }
    }
}