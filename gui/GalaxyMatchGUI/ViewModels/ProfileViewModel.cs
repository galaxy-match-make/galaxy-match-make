using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class ProfileViewModel : ViewModelBase
    {
        private readonly INavigationService? _navigationService;
        private const string ApiBaseUrl = "http://localhost:5284";
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

        // Parameterless constructor for use by the navigation system
        public ProfileViewModel()
        {
            // Get navigation service from the app's service provider
            _navigationService = App.ServiceProvider?.GetService(typeof(INavigationService)) as INavigationService;
            LoadData();
        }

        // Constructor for manual instantiation with dependency injection
        public ProfileViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoadData();
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