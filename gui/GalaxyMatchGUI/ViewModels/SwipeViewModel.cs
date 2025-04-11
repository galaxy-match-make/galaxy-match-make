using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.ViewModels
{
    public class SwipeViewModel : ReactiveObject
    {
        private Profile _currentProfile;
        private List<Profile> _potentialMatches;
        private int _currentIndex = 0;

        // Commands for swipe actions
        public ReactiveCommand<Unit, Unit> SwipeLeftCommand { get; }
        public ReactiveCommand<Unit, Unit> SwipeSuperCommand { get; }
        public ReactiveCommand<Unit, Unit> SwipeRightCommand { get; }
        public ReactiveCommand<Unit, Unit> ViewProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> ViewMessagesCommand { get; }

        public SwipeViewModel()
        {
            // Initialize commands
            SwipeLeftCommand = ReactiveCommand.Create(SwipeLeft);
            SwipeSuperCommand = ReactiveCommand.Create(SwipeSuper);
            SwipeRightCommand = ReactiveCommand.Create(SwipeRight);
            ViewProfileCommand = ReactiveCommand.Create(ViewProfile);
            ViewMessagesCommand = ReactiveCommand.Create(ViewMessages);

            // Load sample data for testing
            LoadSampleData();
        }

        // Properties with reactive notifications
        public Profile CurrentProfile
        {
            get => _currentProfile;
            private set => this.RaiseAndSetIfChanged(ref _currentProfile, value);
        }

        // Methods for swipe actions
        private void SwipeLeft()
        {
            // Logic for rejecting the current profile
            MoveToNextProfile();
        }

        private void SwipeSuper()
        {
            // Logic for super liking the current profile
            MoveToNextProfile();
        }

        private void SwipeRight()
        {
            // Logic for liking the current profile
            MoveToNextProfile();
        }

        private void ViewProfile()
        {
            // Logic to navigate to user's own profile page
            // This is a placeholder for future functionality
        }

        private void ViewMessages()
        {
            // Logic to navigate to messages page
        }

        private void MoveToNextProfile()
        {
            
             _currentIndex++;
             if (_currentIndex < _potentialMatches.Count)
             {
                CurrentProfile = _potentialMatches[_currentIndex];
             }
             else
             {
                    _currentIndex = 0;
                    CurrentProfile = _potentialMatches[_currentIndex];
             }
        }


        // Sample data method - we need to fetch from a service
        private void LoadSampleData()
        {
            _potentialMatches = new List<Profile>
            {
                new Profile
                {
                    DisplayName = "Groot",
                    GalacticDateOfBirth = 129,
                    Bio = "Greetings, Earth dwellers! I am Groot!. I enjoy quantum physics, telepathic chess, and long floats in the ammonia seas. Looking for someone who appreciates tentacle humor and doesn't mind occasional shape-shifting.",
                    AvatarUrl = "avares://GalaxyMatchGUI/Assets/alien_profile.png",
                    HeightInGalacticInches = 86.4f, // 7.2 feet
                    Species = new Species { SpeciesName = "Zorgonian" },
                    Planet = new Planet { PlanetName = "Zorgon Prime" },
                    User = new User
                    {
                        Attributes = new List<UserAttribute>
                        {
                            new UserAttribute { AttributeId = 1 }, // Telepathic
                            new UserAttribute { AttributeId = 2 }  // 3 Hearts
                        }.AsReadOnly(),
                        Interests = new List<UserInterest>
                        {
                            new UserInterest { Interest = new Interest { InterestName = "Cosmic Travel" } },
                            new UserInterest { Interest = new Interest { InterestName = "Telepathy" } },
                            new UserInterest { Interest = new Interest { InterestName = "Human Studies" } }
                        }.AsReadOnly()
                    }
                },
                // Add more sample profiles as needed
            };

            // Set the initial profile
            if (_potentialMatches.Count > 0)
            {
                CurrentProfile = _potentialMatches[0];
            }
        }
    }
}