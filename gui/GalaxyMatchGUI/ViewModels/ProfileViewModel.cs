using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private Profile _profile = new Profile();
        private string _displayName = string.Empty;
        private string _bio = string.Empty;
        private string _avatarUrl = string.Empty;
        private Bitmap? _avatarImage;
        private Species? _selectedSpecies;
        private Planet? _selectedPlanet;
        private Gender? _selectedGender;
        private string _newInterestName = string.Empty;
        private bool _isLoading;

        public ProfileViewModel()
        {
            // Initialize with mock data in a real app this would come from a service
            InitializeMockData();
            
            // Commands
            GoBackCommand = ReactiveCommand.Create(GoBack);
            ChangeAvatarCommand = ReactiveCommand.Create(ChangeAvatar);
            AddInterestCommand = ReactiveCommand.Create(AddInterest);
            SaveChangesCommand = ReactiveCommand.CreateFromTask(SaveChangesAsync);
            LogoutCommand = ReactiveCommand.Create(Logout);
        }

        public ProfileViewModel(Profile profile, string speciesName, string planetName, string genderName, IEnumerable<Interest> interests,
            IEnumerable<UserAttribute> attributes) : this()
        {
            _profile = profile;
            _displayName = profile.DisplayName;
            _bio = profile.Bio;
            _avatarUrl = profile.AvatarUrl;
            
            // Find and set selected items from the available collections
            SelectedSpecies = AvailableSpecies.FirstOrDefault(s => s.SpeciesName == speciesName);
            SelectedPlanet = AvailablePlanets.FirstOrDefault(p => p.PlanetName == planetName);
            SelectedGender = AvailableGenders.FirstOrDefault(g => g.GenderName == genderName);
            
            // Set selected interests and attributes
            foreach (var interest in interests)
            {
                var availableInterest = AvailableInterests.FirstOrDefault(i => i.Id == interest.Id);
                if (availableInterest != null)
                    availableInterest.IsSelected = true;
            }
            
            foreach (var attribute in attributes)
            {
                var availableAttribute = AvailableAttributes.FirstOrDefault(a => a.AttributeId == attribute.AttributeId);
                if (availableAttribute != null)
                    availableAttribute.IsSelected = true;
            }
        }

        // Properties for two-way binding
        public string DisplayName
        {
            get => _displayName;
            set => this.SetProperty(ref _displayName, value);
        }

        public string Bio
        {
            get => _bio;
            set => this.SetProperty(ref _bio, value);
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set
            {
                this.SetProperty(ref _avatarUrl, value);
                _avatarImage = LoadAvatarImage();
                OnPropertyChanged(nameof(AvatarImage));
            }
        }

        public Bitmap? AvatarImage => _avatarImage ?? (_avatarImage = LoadAvatarImage());

        public bool IsLoading
        {
            get => _isLoading;
            set => this.SetProperty(ref _isLoading, value);
        }

        public string NewInterestName
        {
            get => _newInterestName;
            set => this.SetProperty(ref _newInterestName, value);
        }

        // Collections for available options
        public ObservableCollection<SelectableSpecies> AvailableSpecies { get; } = new ObservableCollection<SelectableSpecies>();
        public ObservableCollection<SelectablePlanet> AvailablePlanets { get; } = new ObservableCollection<SelectablePlanet>();
        public ObservableCollection<SelectableGender> AvailableGenders { get; } = new ObservableCollection<SelectableGender>();
        public ObservableCollection<SelectableAttribute> AvailableAttributes { get; } = new ObservableCollection<SelectableAttribute>();
        public ObservableCollection<SelectableInterest> AvailableInterests { get; } = new ObservableCollection<SelectableInterest>();

        // Selected items
        public Species? SelectedSpecies
        {
            get => _selectedSpecies;
            set => this.SetProperty(ref _selectedSpecies, value);
        }

        public Planet? SelectedPlanet
        {
            get => _selectedPlanet;
            set => this.SetProperty(ref _selectedPlanet, value);
        }

        public Gender? SelectedGender
        {
            get => _selectedGender;
            set => this.SetProperty(ref _selectedGender, value);
        }

        // Commands
        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public ReactiveCommand<Unit, Unit> ChangeAvatarCommand { get; }
        public ReactiveCommand<Unit, Unit> AddInterestCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        // Command methods
        private void GoBack()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var swipeView = new SwipeView();
                App.NavigationService.NavigateToWindow(swipeView);
            });
        }

        private void ChangeAvatar()
        {
            // In a real app, this would open a file dialog
            // For now, we'll just rotate through a few preset avatars
            string[] avatars = 
            {
                "avares://GalaxyMatchGUI/Assets/alien_profile.png",
                "avares://GalaxyMatchGUI/Assets/ufo.png"
            };
            
            int currentIndex = Array.IndexOf(avatars, AvatarUrl);
            int nextIndex = (currentIndex + 1) % avatars.Length;
            AvatarUrl = avatars[nextIndex];
        }

        private void AddInterest()
        {
            if (string.IsNullOrWhiteSpace(NewInterestName))
                return;

            // Add the new interest to the available interests list
            var newInterest = new SelectableInterest
            {
                Id = AvailableInterests.Count + 1,
                InterestName = NewInterestName,
                IsSelected = true
            };
            
            AvailableInterests.Add(newInterest);
            NewInterestName = string.Empty;
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;

                // Simulate network delay
                await Task.Delay(1500);

                // In a real app, this would save changes to a database or API
                
                // Update the profile with new values
                _profile.DisplayName = DisplayName;
                _profile.Bio = Bio;
                _profile.AvatarUrl = AvatarUrl;
                
                // Show successful save message (in a real app you might use a toast notification)
            }
            catch (Exception ex)
            {
                // Handle errors (in a real app you might show an error message)
                Console.WriteLine($"Error saving profile: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Logout()
        {
            // Reset the shared profile instance
            ProfileManager.Reset();
            
            // Navigate back to login screen
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var loginView = new LoginView();
                App.NavigationService.NavigateToWindow(loginView);
            });
        }

        private Bitmap? LoadAvatarImage()
        {
            try
            {
                return new Bitmap(_avatarUrl); // Assumes AvatarUrl is a valid local path or resource URI
            }
            catch
            {
                // Return a default image on failure
                try
                {
                    return new Bitmap("avares://GalaxyMatchGUI/Assets/alien_profile.png");
                }
                catch
                {
                    return null;
                }
            }
        }

        private void InitializeMockData()
        {
            // Mock profile data
            _profile = new Profile
            {
                Id = 1,
                DisplayName = "Zorgoth",
                Bio = "Galactic explorer seeking companionship for interstellar adventures. Enjoys nebula surfing and black hole photography.",
                AvatarUrl = "avares://GalaxyMatchGUI/Assets/alien_profile.png",
                GalacticDateOfBirth = 3023
            };
            
            _displayName = _profile.DisplayName;
            _bio = _profile.Bio;
            _avatarUrl = _profile.AvatarUrl;

            // Add available species
            AvailableSpecies.Add(new SelectableSpecies { Id = 1, SpeciesName = "Zorblaxian" });
            AvailableSpecies.Add(new SelectableSpecies { Id = 2, SpeciesName = "Human" });
            AvailableSpecies.Add(new SelectableSpecies { Id = 3, SpeciesName = "Martian" });
            AvailableSpecies.Add(new SelectableSpecies { Id = 4, SpeciesName = "Neptunian" });
            
            // Add available planets
            AvailablePlanets.Add(new SelectablePlanet { Id = 1, PlanetName = "Earth" });
            AvailablePlanets.Add(new SelectablePlanet { Id = 2, PlanetName = "Mars" });
            AvailablePlanets.Add(new SelectablePlanet { Id = 3, PlanetName = "Zorblax Prime" });
            AvailablePlanets.Add(new SelectablePlanet { Id = 4, PlanetName = "Kepler-22b" });
            
            // Add available genders
            AvailableGenders.Add(new SelectableGender { Id = 1, GenderName = "Male" });
            AvailableGenders.Add(new SelectableGender { Id = 2, GenderName = "Female" });
            AvailableGenders.Add(new SelectableGender { Id = 3, GenderName = "Non-Binary" });
            AvailableGenders.Add(new SelectableGender { Id = 4, GenderName = "Quantum Fluid" });
            
            // Add available attributes
            AvailableAttributes.Add(new SelectableAttribute { AttributeId = 1, IsSelected = true });
            AvailableAttributes.Add(new SelectableAttribute { AttributeId = 2, IsSelected = false });
            AvailableAttributes.Add(new SelectableAttribute { AttributeId = 3, IsSelected = true });
            AvailableAttributes.Add(new SelectableAttribute { AttributeId = 4, IsSelected = false });
            
            // Add available interests
            AvailableInterests.Add(new SelectableInterest { Id = 1, InterestName = "Nebula Surfing", IsSelected = true });
            AvailableInterests.Add(new SelectableInterest { Id = 2, InterestName = "Black Hole Photography", IsSelected = true });
            AvailableInterests.Add(new SelectableInterest { Id = 3, InterestName = "Anti-Gravity Yoga", IsSelected = false });
            AvailableInterests.Add(new SelectableInterest { Id = 4, InterestName = "Intergalactic Cooking", IsSelected = false });
            
            // Set default selections
            _selectedSpecies = AvailableSpecies[0];
            _selectedPlanet = AvailablePlanets[2];
            _selectedGender = AvailableGenders[0];
        }
    }

    // Selectable wrapper classes for data binding
    public class SelectableSpecies : Species
    {
        public bool IsSelected { get; set; }
    }

    public class SelectablePlanet : Planet
    {
        public bool IsSelected { get; set; }
    }

    public class SelectableGender : Gender
    {
        public bool IsSelected { get; set; }
    }

    public class SelectableAttribute : UserAttribute
    {
        public bool IsSelected { get; set; }
    }

    public class SelectableInterest : Interest
    {
        public bool IsSelected { get; set; }
    }
}
