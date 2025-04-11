using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public LoginViewModel()
        {
            GoogleSignInCommand = ReactiveCommand.CreateFromTask(SignInWithGoogleAsync, this.WhenAnyValue(x => x.IsLoading, isLoading => !isLoading));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Command for Google Sign In
        public ReactiveCommand<Unit, Unit> GoogleSignInCommand { get; }

        // Method that will be executed when the command is triggered
        private async Task SignInWithGoogleAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Contacting galactic servers...";

                // Simulate network delay
                await Task.Delay(2000);

                // Simulate login success
                StatusMessage = "Login successful! Navigating through the wormhole...";

                // Add a small delay to show the success message before navigating
                await Task.Delay(1000);
                
                // Navigate to the SwipeView using window-based navigation
                var swipeView = new SwipeView();
                App.NavigationService.NavigateToWindow(swipeView);
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Login Failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}