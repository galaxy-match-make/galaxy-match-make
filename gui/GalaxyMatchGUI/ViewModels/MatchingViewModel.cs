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

public partial class MatchingViewModel : ViewModelBase
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
    private readonly InteractionsViewModel _interactionsViewModel;
    
    private int _currentProfileIndex = 0;

    public IRelayCommand SwipeLeftCommand { get; }
    public IRelayCommand SwipeRightCommand { get; }
    public IRelayCommand ViewProfileCommand { get; }
    public IRelayCommand ViewMessagesCommand { get; }

    public MatchingViewModel()
    {
        _reactionService = new ReactionService();
        _profileService = new ProfileService();
        _interactionsViewModel = new InteractionsViewModel();
        SwipeLeftCommand = new RelayCommand(SwipeLeft);
        SwipeRightCommand = new RelayCommand(SwipeRight);
        ViewProfileCommand = new RelayCommand(ViewProfile);
        ViewMessagesCommand = new RelayCommand(ViewMessages);

        IsLoading = true;
        StatusMessage = string.Empty;;
        
        _ = LoadAllProfiles();
    }

    private async Task LoadAllProfiles()
    {
        IsLoading = true;
        
        await Task.Delay(2200);
        
        try
        {
            var jwtToken = JwtStorage.Instance.authDetails?.JwtToken;
            if (string.IsNullOrEmpty(jwtToken))
            {
                StatusMessage = "You must be logged in to find matches";
                NavigationService?.NavigateTo<LoginViewModel>();
                return;
            }
            
            _allProfiles = await _profileService.GetAllProfilesAsync();
            
            
            if (_allProfiles != null && _allProfiles.Any())
            {
                var currentUserId = JwtStorage.Instance.authDetails?.UserId;
                var contactsConnected = await _interactionsViewModel.GetMessageContactsAsync();
                var usersSentRequestsTo = await _interactionsViewModel.GetSentRequestContacts();

                var requestsSent = usersSentRequestsTo.Select(request => request.UserId).ToList();

                var matchedProfiles = contactsConnected.Select(message => message.UserId).ToList();
                
                _allProfiles = _allProfiles.Where(profile => profile.UserId != currentUserId && !matchedProfiles.Contains(profile.UserId) && !requestsSent.Contains(profile.UserId)).ToList();

                await ShowNextProfile();
            }
            else
            {
                // TODO: No profiles found, show no profiles message
                
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading profiles, please try again";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task ShowNextProfile()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        
        if (_allProfiles == null || !_allProfiles.Any())
        {
            // TODO : Fall back to no profiles message
            IsLoading = false;
            return;
        }
        
        if (_currentProfileIndex >= _allProfiles.Count)
        {
            // TODO: Show no more profiles message
            _currentProfileIndex = 0;
        }
        
        var profile = _allProfiles[_currentProfileIndex++];
        CurrentProfile = profile;
        
        await LoadProfileImage(profile.AvatarUrl);
        LoadProfileAttributes(profile);
        
        IsLoading = false;
    }
    
    private void LoadProfileAttributes(Profile profile)
    {
        PhysicalAttributes.Clear();
        
        if (profile.Species != null)
        {
            PhysicalAttributes.Add(new PhysicalAttribute { Icon = "👽", Description = profile.Species.SpeciesName });
        }
        
        if (profile.Planet != null)
        {
            PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🪐", Description = $"From {profile.Planet.PlanetName}" });
        }
        
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
        
        if (profile.HeightInGalacticInches.HasValue)
        {
            PhysicalAttributes.Add(new PhysicalAttribute 
            { 
                Icon = "📏", 
                Description = $"{profile.HeightInGalacticInches.Value} galactic inches" 
            });
        }
        
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
            StatusMessage = "Failed to record your reaction";
        }
    }
    
    private async Task AnimateSwipe(double translateX, double rotation)
    {
        CardRotation = rotation;
        CardTranslateX = translateX;
        CardOpacity = 0;
        
        await Task.Delay(300);
        
        CardTranslateX = 0;
        CardRotation = 0;
        CardOpacity = 0;
        
        if (_allProfiles.Any())
        {
            await ShowNextProfile();
        }
        else
        {
            // TODO: Shpow no more profiles message
            await LoadNextProfile();
        }
        
        CardScale = 0.95;
        await Task.Delay(50);
        
        CardOpacity = 1;
        CardScale = 1;
    }

    private void ViewProfile()
    {
        
        var profileViewModel = new ProfileViewModel
        {
            IsEditMode = true
        };
        
        NavigationService?.NavigateTo(profileViewModel);
    }

    private void ViewMessages()
    {
        NavigationService?.NavigateTo<InteractionsViewModel>();
    }
    
    private async Task LoadNextProfile()
    {
        IsLoading = true;
        
        await LoadProfileWithBase64Image();
        
        IsLoading = false;
        StatusMessage = string.Empty;
    }

    private async Task LoadProfileWithBase64Image()
    {
        var profile = CreateBasicProfile();
        
        string sampleBase64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAABHNCSVQICAgIfAhkiAAAAUdJREFUeJzt1DEBwCAAwDB0voIzhD0EBDkTBTUNmTfHP1nXdR5+besDPM0AMQPEDBBb5w1vt5+orV4HiAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIGaAmAFiBogZIPYBmuwEO61WpfIAAAAASUVORK5CYII=";
        
        await LoadBase64Image(sampleBase64Image);
        
        CurrentProfile = profile;
        LoadProfileData();
    }
    
    private Profile CreateBasicProfile()
    {
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
                AvatarImage = await AsyncImageLoader.LoadAsync(avatarUrl);
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
            StatusMessage = $"Error loading image";
            
            try
            {
                var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                using var stream = AssetLoader.Open(uri);
                AvatarImage = new Bitmap(stream);
            }
            catch (Exception fallbackEx)
            {
                StatusMessage = "Unable to load image";
            }
        }
    }
    
    private async Task LoadBase64Image(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            await LoadProfileImage("avares://GalaxyMatchGUI/Assets/alien_profile.png");
            return;
        }

        try
        {
            IsBase64Image = true;
            
            string sanitizedBase64 = base64String;
            if (base64String.Contains(","))
            {
                sanitizedBase64 = base64String.Substring(base64String.IndexOf(',') + 1);
            }
        
            byte[] imageBytes = Convert.FromBase64String(sanitizedBase64);
            
            using (var memoryStream = new MemoryStream(imageBytes))
            {
                AvatarImage = new Bitmap(memoryStream);
            }
        }
        catch (Exception ex)
        {
            IsBase64Image = false;
            
            await LoadProfileImage("avares://GalaxyMatchGUI/Assets/alien_profile.png");
        }
    }

    private void LoadProfileData()
    {
        PhysicalAttributes.Clear();
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "👽", Description = "3 Eyes" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🦑", Description = "7 Tentacles" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🌈", Description = "Color-shifting" });
        PhysicalAttributes.Add(new PhysicalAttribute { Icon = "🧠", Description = "Telepathic" });
        
        Interests.Clear();
        Interests.Add(new InterestItem { Name = "Space Exploration" });
        Interests.Add(new InterestItem { Name = "Quantum Physics" });
        Interests.Add(new InterestItem { Name = "Earth Cuisine" });
        Interests.Add(new InterestItem { Name = "Intergalactic Travel" });
        Interests.Add(new InterestItem { Name = "Telepathic Music" });
    }

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