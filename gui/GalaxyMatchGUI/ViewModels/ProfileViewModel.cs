using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GalaxyMatchGUI.lib;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class ProfileViewModel : ViewModelBase
    {
        private readonly INavigationService? _navigationService;
        private string ApiBaseUrl = AppSettings.BackendUrl;
        private readonly HttpClient _httpClient = new HttpClient();

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _bio = string.Empty;

        [ObservableProperty]
        private string _avatarUrl = string.Empty;

        [ObservableProperty]
        private float? _heightInGalacticInches;

        [ObservableProperty]
        private int? _galacticDateOfBirth;

        [ObservableProperty]
        private ObservableCollection<Planet> _planets = new();

        [ObservableProperty]
        private Planet? _selectedPlanet;

        [ObservableProperty]
        private ObservableCollection<Species> _species = new();

        [ObservableProperty]
        private Species? _selectedSpecies;

        [ObservableProperty]
        private ObservableCollection<Gender> _genders = new();

        [ObservableProperty]
        private Gender? _selectedGender;

        [ObservableProperty]
        private ObservableCollection<Interest> _allInterests = new();

        [ObservableProperty]
        private ObservableCollection<Interest> _selectedInterests = new();

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private Profile? _existingProfile;

        [ObservableProperty]
        private string uploadStatusMessage;

        [ObservableProperty]
        private Bitmap avatarImage;

        // Parameterless constructor for use by the navigation system
        public ProfileViewModel()
        {
            // Get navigation service from the app's service provider
            _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
            LoadData();

        }

        [RelayCommand]
        private async Task UploadImage()
        {
            try
            {
                // Get the current window directly from the application lifetime
                var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (window == null)
                {
                    UploadStatusMessage = "Cannot access file system";
                    return;
                }
                
                // Create file picker options
                var filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter
                    {
                        Name = "Image Files",
                        Extensions = new List<string> { "jpg", "jpeg", "png", "gif", "bmp" }
                    }
                };
                
                // Open file dialog
                var dialog = new OpenFileDialog
                {
                    Title = "Select Profile Image",
                    Filters = filters,
                    AllowMultiple = false
                };
                
                var result = await dialog.ShowAsync(window);
                if (result == null || result.Length == 0)
                {
                    UploadStatusMessage = "";
                    return;
                }
                
                var filePath = result[0];
                
                // Check file size
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 1024 * 1024 * 2) // 2MB max
                {
                    UploadStatusMessage = "Image too large (max 2MB)";
                    return;
                }
                
                // Read the file as bytes
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                
                // Convert to base64
                string base64Image = Convert.ToBase64String(fileBytes);
                
                // Determine file extension
                string fileName = Path.GetFileName(filePath).ToLower();
                string mimeType = "image/jpeg"; // Default
                
                if (fileName.EndsWith(".png"))
                    mimeType = "image/png";
                else if (fileName.EndsWith(".gif"))
                    mimeType = "image/gif";
                else if (fileName.EndsWith(".bmp"))
                    mimeType = "image/bmp";
                
                // Create the data URL format
                AvatarUrl = $"data:{mimeType};base64,{base64Image}";

                await LoadAvatarImageAsync();
                
                UploadStatusMessage = "Image uploaded successfully";
                
            }
            catch (Exception ex)
            {
                UploadStatusMessage = $"Error uploading image: {ex.Message}";
                Console.WriteLine($"Image upload error: {ex}");
            }
        }

        private async Task LoadAvatarImageAsync()
        {
            if (string.IsNullOrEmpty(AvatarUrl))
            {
                // Load default avatar
                AvatarImage = new Bitmap("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                return;
            }

            try
            {
                int commaIndex = AvatarUrl.IndexOf(',');
                if (commaIndex > 0)
                {
                    string data = AvatarUrl.Substring(commaIndex + 1);
                    byte[] bytes = Convert.FromBase64String(data);
                    
                    using var stream = new MemoryStream(bytes);
                    AvatarImage = new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading avatar: {ex.Message}");
                // Fallback to default image
                AvatarImage = new Bitmap("avares://GalaxyMatchGUI/Assets/alien_profile.png");
            }
        }

        public ProfileViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoadData();
        }

        private async Task LoadProfileImage(string? avatarUrl)
        {
            if (string.IsNullOrEmpty(avatarUrl))
            {
                // Use default image
                avatarUrl = "avares://GalaxyMatchGUI/Assets/alien_profile.png";
            }
            
            try
            {
                // Check if it's a base64 image
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
                    // Load from remote URL
                    var image = await AsyncImageLoader.LoadAsync(avatarUrl);
                    AvatarImage = (Bitmap)image;
                }
                else
                {
                    // Load from local asset
                    var uri = new Uri(avatarUrl);
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile image: {ex.Message}");
                
                try
                {
                    // If URL loading fails, try fall back to local asset
                    var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Error loading fallback image: {fallbackEx.Message}");
                }
            }
        }

        private async void LoadData()
        {
            IsLoading = true;
            StatusMessage = "Loading galactic data...";

            try
            {
                // Set up HTTP client with JWT
                var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }

                // Load planets, species, genders, and interests
                await Task.WhenAll(
                    LoadPlanets(),
                    LoadSpecies(),
                    LoadGenders(),
                    LoadInterests(),
                    LoadCurrentProfile()
                );
                
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadBase64Image(string base64String)
        {
            try
            {
                // Extract the base64 part after the comma
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
                Console.WriteLine($"Error loading base64 image: {ex.Message}");
                
                // Load fallback image on error
                try
                {
                    var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                    using var stream = AssetLoader.Open(uri);
                    AvatarImage = new Bitmap(stream);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"Error loading fallback image: {fallbackEx.Message}");
                }
            }
        }


        private async Task LoadCurrentProfile()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Profile/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var profile = JsonSerializer.Deserialize<Profile>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (profile != null)
                    {
                        ExistingProfile = profile;
                        IsEditMode = true;
                        
                        // Populate form with existing data
                        DisplayName = profile.DisplayName;
                        Bio = profile.Bio ?? string.Empty;
                        AvatarUrl = profile.AvatarUrl ?? string.Empty;
                        await LoadProfileImage(AvatarUrl);
                        HeightInGalacticInches = profile.HeightInGalacticInches;
                        GalacticDateOfBirth = profile.GalacticDateOfBirth;
                        
                        // Set selected values
                        if (profile.Planet != null && Planets.Any())
                            SelectedPlanet = Planets.FirstOrDefault(p => p.Id == profile.PlanetId);
                        
                        if (profile.Species != null && Species.Any())
                            SelectedSpecies = Species.FirstOrDefault(s => s.Id == profile.SpeciesId);
                        
                        if (profile.Gender != null && Genders.Any())
                            SelectedGender = Genders.FirstOrDefault(g => g.Id == profile.GenderId);
                        
                        // Set selected interests
                        if (profile.UserInterests != null && profile.UserInterests.Any() && AllInterests.Any())
                        {
                            SelectedInterests.Clear();
                            foreach (var userInterest in profile.UserInterests)
                            {
                                var interest = AllInterests.FirstOrDefault(i => i.Id == userInterest.InterestId);
                                if (interest != null)
                                    SelectedInterests.Add(interest);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading profile: {ex.Message}");
                // No profile exists yet, that's okay for new users
            }
        }

        private async Task LoadPlanets()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Planets");
                if (response.IsSuccessStatusCode)
                {
                    var planets = await response.Content.ReadFromJsonAsync<List<Planet>>();
                    if (planets != null)
                    {
                        Planets = new ObservableCollection<Planet>(planets);
                        if (Planets.Any())
                            SelectedPlanet = Planets.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading planets: {ex.Message}");
            }
        }

        private async Task LoadSpecies()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Species");
                if (response.IsSuccessStatusCode)
                {
                    var species = await response.Content.ReadFromJsonAsync<List<Species>>();
                    if (species != null)
                    {
                        Species = new ObservableCollection<Species>(species);
                        if (Species.Any())
                            SelectedSpecies = Species.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading species: {ex.Message}");
            }
        }

        private async Task LoadGenders()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Gender");
                if (response.IsSuccessStatusCode)
                {
                    var genders = await response.Content.ReadFromJsonAsync<List<Gender>>();
                    if (genders != null)
                    {
                        Genders = new ObservableCollection<Gender>(genders);
                        if (Genders.Any())
                            SelectedGender = Genders.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading genders: {ex.Message}");
            }
        }

        private async Task LoadInterests()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Interests");
                if (response.IsSuccessStatusCode)
                {
                    var interests = await response.Content.ReadFromJsonAsync<List<Interest>>();
                    if (interests != null)
                    {
                        AllInterests = new ObservableCollection<Interest>(interests);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading interests: {ex.Message}");
            }
        }

        // Fix: Make sure the method signature matches what's expected
        [RelayCommand]
        public void ToggleInterest(Interest interest)
        {
            if (SelectedInterests.Contains(interest))
            {
                SelectedInterests.Remove(interest);
            }
            else
            {
                SelectedInterests.Add(interest);
            }
            
            // Force UI update (might help in some cases)
            OnPropertyChanged(nameof(SelectedInterests));
        }

        [RelayCommand]
        public async Task SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                StatusMessage = "Display name is required";
                return;
            }

            IsLoading = true;
            StatusMessage = "Saving your cosmic profile...";

            try
            {
                var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
                if (string.IsNullOrEmpty(jwtToken))
                {
                    StatusMessage = "You must be logged in";
                    IsLoading = false;
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                // Create profile object
                var profileData = new
                {
                    DisplayName = DisplayName,
                    Bio = Bio,
                    AvatarUrl = AvatarUrl,
                    HeightInGalacticInches = HeightInGalacticInches,
                    GalacticDateOfBirth = GalacticDateOfBirth,
                    SpeciesId = SelectedSpecies?.Id,
                    PlanetId = SelectedPlanet?.Id,
                    GenderId = SelectedGender?.Id,
                    UserInterestIds = SelectedInterests.Select(i => i.Id).ToList()
                };

                HttpResponseMessage response;
                
                if (IsEditMode)
                {
                    // Update existing profile
                    response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/api/Profile", profileData);
                }
                else
                {
                    // Create new profile
                    response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/Profile", profileData);
                }

                if (response.IsSuccessStatusCode)
                {
                    StatusMessage = "Profile saved successfully!";
                    // Navigate to the matching view after successful save
                    _navigationService?.NavigateTo<MatchingViewModel>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    StatusMessage = $"Error saving profile: {error}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving profile: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void Cancel()
        {
            // Navigate back to login/previous view
            _navigationService?.NavigateBack();
        }
    }
}