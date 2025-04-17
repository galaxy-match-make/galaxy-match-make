using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.lib;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class ViewOtherProfileViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string ApiBaseUrl = AppSettings.BackendUrl;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _bio = string.Empty;

        [ObservableProperty]
        private string _avatarUrl = string.Empty;

        [ObservableProperty]
        private Bitmap _avatarImage;

        [ObservableProperty]
        private float? _heightInGalacticInches;

        [ObservableProperty]
        private int? _galacticDateOfBirth;

        [ObservableProperty]
        private int _galacticAge;

        [ObservableProperty]
        private string _planetName = string.Empty;

        [ObservableProperty]
        private string _speciesName = string.Empty;

        [ObservableProperty]
        private string _genderName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Interest> _userInterests = new();

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _profileSummary = string.Empty;

        public ViewOtherProfileViewModel(Guid userId)
        {
            LoadProfileData(userId);
        }

        private async void LoadProfileData(Guid userId)
        {
            IsLoading = true;
            StatusMessage = "Scanning galactic records...";

            try
            {
                var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
                
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Profile/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var profile = JsonSerializer.Deserialize<Profile>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (profile != null)
                    {
                        DisplayName = profile.DisplayName;
                        Bio = profile.Bio ?? string.Empty;
                        AvatarUrl = profile.AvatarUrl ?? string.Empty;
                        await LoadProfileImage(AvatarUrl);
                        HeightInGalacticInches = profile.HeightInGalacticInches;
                        GalacticDateOfBirth = profile.GalacticDateOfBirth;
                        GalacticAge = profile.GalacticDateOfBirth.HasValue ? profile.GalacticDateOfBirth.Value : 0;
                        
                        PlanetName = profile.Planet?.PlanetName ?? "Unknown";
                        SpeciesName = profile.Species?.SpeciesName ?? "Unknown";
                        GenderName = profile.Gender?.GenderName ?? "Unknown";
                        
                        ProfileSummary = $"{SpeciesName} from {PlanetName}";
                        
                        if (profile.UserInterests != null && profile.UserInterests.Any())
                        {
                            UserInterests.Clear();
                            foreach (var userInterest in profile.UserInterests)
                            {
                                if (userInterest.Interest != null)
                                {
                                    UserInterests.Add(userInterest.Interest);
                                }
                            }
                        }
                    }
                }
                else
                {
                    StatusMessage = "Could not load profile data";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "We were unable to load profile data";
            }
            finally
            {
                IsLoading = false;
                StatusMessage = string.Empty;
            }
        }

        private async Task LoadProfileImage(string? avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = "avares://GalaxyMatchGUI/Assets/alien_profile.png";
            }
            
            try
            {
                if (avatarUrl.StartsWith("data:image") && avatarUrl.Contains("base64,"))
                {
                    await LoadBase64Image(avatarUrl);
                    return;
                }
                
                bool hasValidUrl = !string.IsNullOrEmpty(avatarUrl) && 
                                (avatarUrl.StartsWith("http://") || 
                                avatarUrl.StartsWith("https://"));
                                
                if (hasValidUrl)
                {
                    var image = await AsyncImageLoader.LoadAsync(avatarUrl);
                    AvatarImage = (Bitmap)image;
                }
                else
                {
                    var uri = new Uri(avatarUrl);
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                
                try
                {
                    var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
                catch (Exception fallbackEx)
                {
                    StatusMessage = "Could not load avatar image";
                }
            }
        }

        private async Task LoadBase64Image(string base64String)
        {
            try
            {
                int commaIndex = base64String.IndexOf(',');
                if (commaIndex > 0)
                {
                    string data = base64String.Substring(commaIndex + 1);
                    byte[] bytes = Convert.FromBase64String(data);
                    
                    using var stream = new MemoryStream(bytes);
                    AvatarImage = new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
                catch (Exception fallbackEx)
                {
                    StatusMessage = "Could not load avatar image";
                }
            }
        }

        [RelayCommand]
        public void GoBack()
        {
            NavigationService?.NavigateTo<InteractionsViewModel>();
        }
    }
}