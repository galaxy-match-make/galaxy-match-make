using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.lib;

namespace GalaxyMatchGUI.Services
{
    public class ProfileService
    {
        private readonly HttpClient _httpClient;
        private string ApiBaseUrl = AppSettings.BackendUrl;

        public ProfileService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
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

                Console.WriteLine("Making request to API endpoint: " + $"{ApiBaseUrl}/api/Profile");
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Profile");
                Console.WriteLine($"API Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response JSON: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");
                    
                    try {
                        var options = new System.Text.Json.JsonSerializerOptions { 
                            PropertyNameCaseInsensitive = true
                        };
                        var profiles = System.Text.Json.JsonSerializer.Deserialize<List<Profile>>(jsonContent, options);
                        Console.WriteLine($"Successfully deserialized {profiles?.Count ?? 0} profiles");
                        
                        if (profiles != null && profiles.Any())
                        {
                            foreach (var profile in profiles.Take(2))
                            {
                                Console.WriteLine($"Profile: {profile.DisplayName}, Species: {profile.Species?.SpeciesName}, Interests: {profile.UserInterests?.Count ?? 0}");
                            }
                        }
                        
                        return profiles ?? new List<Profile>();
                    }
                    catch (Exception jsonEx) {
                        Console.WriteLine($"JSON Deserialization error: {jsonEx.Message}");
                        Console.WriteLine($"JSON Deserialization stack trace: {jsonEx.StackTrace}");
                        return new List<Profile>();
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get profiles: {errorContent}");
                    return new List<Profile>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllProfilesAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<Profile> GetProfileByIdAsync(Guid userId)
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

                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Profile/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<Profile>();
                    return profile;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to get profile: {errorContent}");
                    throw new HttpRequestException($"Failed to get profile: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetProfileByIdAsync: {ex.Message}");
                throw;
            }
        }
    }
}