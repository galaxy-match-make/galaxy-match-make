using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.ViewModels;

public class MatchingViewModel : ViewModelBase
{
    private Profile? _currentProfile;
    public Profile? CurrentProfile
    {
        get => _currentProfile;
        set => SetProperty(ref _currentProfile, value);
    }

    private IImage? _avatarImage;
    public IImage? AvatarImage
    {
        get => _avatarImage;
        set => SetProperty(ref _avatarImage, value);
    }
    
    // Animation properties for swipe effects
    private double _cardTranslateX;
    public double CardTranslateX
    {
        get => _cardTranslateX;
        set => SetProperty(ref _cardTranslateX, value);
    }
    
    private double _cardRotation;
    public double CardRotation
    {
        get => _cardRotation;
        set => SetProperty(ref _cardRotation, value);
    }
    
    private double _cardScale = 1.0;
    public double CardScale
    {
        get => _cardScale;
        set => SetProperty(ref _cardScale, value);
    }
    
    private double _cardOpacity = 1.0;
    public double CardOpacity
    {
        get => _cardOpacity;
        set => SetProperty(ref _cardOpacity, value);
    }
    
    private bool _isBase64Image;
    public bool IsBase64Image
    {
        get => _isBase64Image;
        set => SetProperty(ref _isBase64Image, value);
    }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<PhysicalAttribute> PhysicalAttributes { get; } = new();
    public ObservableCollection<InterestItem> Interests { get; } = new();
    
    private List<Profile> _allProfiles = new List<Profile>();

    private readonly ReactionService _reactionService;
    private readonly ProfileService _profileService;
    private int _currentProfileIndex = 0;

    public IRelayCommand SwipeLeftCommand { get; }
    public IRelayCommand SwipeRightCommand { get; }
    public IRelayCommand ViewProfileCommand { get; }
    public IRelayCommand ViewMessagesCommand { get; }

    public MatchingViewModel()
    {
        _reactionService = new ReactionService();
        _profileService = new ProfileService();
        SwipeLeftCommand = new RelayCommand(SwipeLeft);
        SwipeRightCommand = new RelayCommand(SwipeRight);
        ViewProfileCommand = new RelayCommand(ViewProfile);
        ViewMessagesCommand = new RelayCommand(ViewMessages);

        // Load profiles from API with JWT token
        _ = LoadAllProfiles();
    }

    private async Task LoadAllProfiles()
    {
        IsLoading = true;
        StatusMessage = "Finding matches in your galaxy...";
        
        try
        {
            // Check if JWT token is available
            var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
            if (string.IsNullOrEmpty(jwtToken))
            {
                StatusMessage = "You must be logged in to find matches";
                return;
            }
            
            // Get all profiles from the API
            _allProfiles = await _profileService.GetAllProfilesAsync();
            
            if (_allProfiles != null && _allProfiles.Any())
            {
                // Start showing the first profile
                await ShowNextProfile();
            }
            else
            {
                // No profiles found, show fallback
                StatusMessage = "No profiles found in your galaxy";
                await LoadProfileWithBase64Image();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profiles: {ex.Message}";
            // Fall back to demo profile
            await LoadProfileWithBase64Image();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task ShowNextProfile()
    {
        if (_allProfiles == null || !_allProfiles.Any())
        {
            StatusMessage = "No more profiles to show";
            return;
        }
        
        // Check if we've shown all profiles
        if (_currentProfileIndex >= _allProfiles.Count)
        {
            StatusMessage = "You've seen all profiles in your galaxy";
            _currentProfileIndex = 0;
        }
        
        // Get next profile
        var profile = _allProfiles[_currentProfileIndex++];
        CurrentProfile = profile;
        
        Console.WriteLine($"Showing profile: {profile.DisplayName}");
        
        // Try to load profile image
        await LoadProfileImage(profile.AvatarUrl);
        
        // Load profile specific data
        LoadProfileAttributes(profile);
        
        StatusMessage = string.Empty;
    }
    
    private void LoadProfileAttributes(Profile profile)
    {
        // Clear physical attributes
        PhysicalAttributes.Clear();
        
        // Add species as first attribute
        if (profile.Species != null)
        {
            PhysicalAttributes.Add(new PhysicalAttribute { Icon = "👽", Description = profile.Species.SpeciesName });
        }
        
        // Add planet as attribute
        if (profile.Planet != null)
        {
            PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🪐", Description = $"From {profile.Planet.PlanetName}" });
        }
        
        // Add gender if available
        if (profile.Gender != null)
        {
            string genderIcon = profile.Gender.GenderName switch
            {
                "Male" => "♂️",
                "Female" => "♀️",
                "Non-Binary" => "⚧️",
                "Fluid" => "🌊",
                _ => "✨"
            };
            PhysicalAttributes.Add(new PhysicalAttribute { Icon = genderIcon, Description = profile.Gender.GenderName });
        }
        
        // Add height if available
        if (profile.HeightInGalacticInches.HasValue)
        {
            PhysicalAttributes.Add(new PhysicalAttribute 
            { 
                Icon = "📏", 
                Description = $"{profile.HeightInGalacticInches.Value} galactic inches" 
            });
        }
        
        // Clear and populate interests from user interests
        Interests.Clear();
        if (profile.UserInterests != null && profile.UserInterests.Any())
        {
            foreach (var interest in profile.UserInterests)
            {
                Interests.Add(new InterestItem { Name = interest.InterestName });
            }
        }
        else
        {
            // Add a placeholder if no interests
            Interests.Add(new InterestItem { Name = "No listed interests" });
        }
    }

    private void SwipeLeft()
    {
        _ = HandleSwipe(-300, -15, false);
    }

    private void SwipeRight()
    {
        _ = HandleSwipe(300, 15, true);
    }
    
    private async Task HandleSwipe(double translateX, double rotation, bool isLike)
    {
        if (CurrentProfile == null || CurrentProfile.UserId == Guid.Empty)
        {
            return;
        }

        await AnimateSwipe(translateX, rotation);
        
        try
        {
            bool success = await _reactionService.SendReactionAsync(
                CurrentProfile.UserId, 
                isLike
            );
            
            if (success)
            {
                StatusMessage = isLike ? "Liked!" : "Passed";
            }
            else
            {
                StatusMessage = "Failed to record your reaction";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
    
    private async Task AnimateSwipe(double translateX, double rotation)
    {
        CardRotation = rotation;
        CardTranslateX = translateX;
        CardOpacity = 0;
        
        await Task.Delay(300);
        
        // Reset values
        CardTranslateX = 0;
        CardRotation = 0;
        CardOpacity = 0;
        
        // If we have profiles from API, show next profile
        if (_allProfiles.Any())
        {
            await ShowNextProfile();
        }
        else
        {
            // Fall back to demo profile
            await LoadNextProfile();
        }
        
        // Fade in with slight scale effect
        CardScale = 0.95;
        await Task.Delay(50);
        
        // Restore to normal
        CardOpacity = 1;
        CardScale = 1;
    }

    private void ViewProfile()
    {
        // Check if there's a profile to view
        if (CurrentProfile == null) return;
        
        // Create a new ProfileViewModel instance
        var profileViewModel = new ProfileViewModel
        {
            IsEditMode = true,
            ExistingProfile = CurrentProfile,
            DisplayName = CurrentProfile.DisplayName,
            Bio = CurrentProfile.Bio,
            AvatarUrl = CurrentProfile.AvatarUrl,
            HeightInGalacticInches = CurrentProfile.HeightInGalacticInches,
            GalacticDateOfBirth = CurrentProfile.GalacticDateOfBirth,
            SelectedPlanet = CurrentProfile.Planet,
            SelectedSpecies = CurrentProfile.Species,
            SelectedGender = CurrentProfile.Gender
        };
        
        // Navigate to the profile view
        NavigationService?.NavigateTo(profileViewModel);
    }

    private void ViewMessages()
    {
        // Navigate to messages view
        NavigationService?.NavigateTo<InteractionsViewModel>();
    }
    
    // Fallback methods for demo profile if API fails
    private async Task LoadNextProfile()
    {
        IsLoading = true;
        StatusMessage = "Finding your next match...";
        
        // In a real app, this would fetch the next profile from a service
        await LoadProfileWithBase64Image();
        
        IsLoading = false;
        StatusMessage = string.Empty;
    }

    private async Task LoadProfileWithBase64Image()
    {
        // Create a profile with base64 image 
        var profile = CreateBasicProfile();
        
        // Use a base64 encoded image for testing
        // This is a simple purple gradient image
        string sampleBase64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAABHNCSVQICAgIfAhkiAAAAUdJREFUeJzt1DEBwCAAwDB0voIzhD0EBDkTBTUNmTfHP1nXdR5+besDPM0AMQPEDBBb5w1vt5+orV4HiAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIPYBmuwEO61WpfIAAAAASUVORK5CYII=";
        
        // Load the base64 image
        await LoadBase64Image(sampleBase64Image);
        
        // Set current profile
        CurrentProfile = profile;
        
        // Load other profile data
        LoadProfileData();
    }
    
    private Profile CreateBasicProfile()
    {
        // Create a basic profile for demo/fallback purposes
        return new Profile
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Zetron-7",
            Bio = "Greetings, Earth dwellers! I come in peace from the Andromeda galaxy. Looking for beings who appreciate quantum harmonics and interstellar travel. I can shape-shift and read minds, but I promise I won't peek without permission!",
            Gender = new Models.Gender { GenderName = "Fluid" },
            Species = new Models.Species { SpeciesName = "Andromedian" },
            Planet = new Models.Planet { PlanetName = "Zetron Prime" },
            HeightInGalacticInches = 152,
            GalacticDateOfBirth = 1535
        };
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
                AvatarImage = await AsyncImageLoader.LoadAsync(avatarUrl);
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
    
    // Dedicated method for handling base64 encoded images
    private async Task LoadBase64Image(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            // Fall back to default image if base64 is empty
            await LoadProfileImage("avares://GalaxyMatchGUI/Assets/alien_profile.png");
            return;
        }

        try
        {
            IsBase64Image = true;
            
            // Remove the data URL prefix if present (e.g., "data:image/jpeg;base64,")
            string sanitizedBase64 = base64String;
            if (base64String.Contains(","))
            {
                sanitizedBase64 = base64String.Substring(base64String.IndexOf(',') + 1);
            }
            
            // Convert base64 to byte array
            byte[] imageBytes = Convert.FromBase64String(sanitizedBase64);
            
            // Create a bitmap from the byte array
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                AvatarImage = new Bitmap(memoryStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading base64 image: {ex.Message}");
            IsBase64Image = false;
            
            // Fall back to default image
            await LoadProfileImage("avares://GalaxyMatchGUI/Assets/alien_profile.png");
        }
    }

    private void LoadProfileData()
    {
        // Clear and populate physical attributes
        PhysicalAttributes.Clear();
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "👽", Description = "3 Eyes" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🦑", Description = "7 Tentacles" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🌈", Description = "Color-shifting" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🧠", Description = "Telepathic" });

        // Clear and populate interests
        Interests.Clear();
        Interests.Add(new InterestItem { Name = "Space Exploration" });
        Interests.Add(new InterestItem { Name = "Quantum Physics" });
        Interests.Add(new InterestItem { Name = "Earth Cuisine" });
        Interests.Add(new InterestItem { Name = "Intergalactic Travel" });
        Interests.Add(new InterestItem { Name = "Telepathic Music" });
    }

    // Helper classes for UI display
    public class PhysicalAttribute
    {
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class InterestItem
    {
        public string Name { get; set; } = string.Empty;
    }
}