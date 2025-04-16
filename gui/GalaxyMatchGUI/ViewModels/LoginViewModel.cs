using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using GalaxyMatchGUI.Services;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isLoggingIn;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public LoginViewModel()
        {
        }

        [RelayCommand]
        public async Task GoogleSignInAsync()
        {
            try
            {
                IsLoggingIn = true;
                StatusMessage = "Contacting galactic servers...";

                var authService = new AuthService();
                var authResponse = await authService.StartLoginFlow();
                
                if (authResponse == null)
                {
                    StatusMessage = "Authentication failed. Please try again.";
                    return;
                }

                Console.WriteLine($"JWT Token: {authResponse.JwtToken}");

                // Check if the user's profile is complete
                if (authResponse.ProfileComplete)
                {
                    // User already has a profile, navigate to the matching view
                    StatusMessage = "Authentication successful!";

                    NavigationService?.NavigateTo<MatchingViewModel>();
                }
                else
                {
                    // User needs to complete their profile
                    StatusMessage = "Please complete your profile";
                    
                    // Navigate to profile creation/edit view
                    NavigationService?.NavigateTo<ProfileViewModel>();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Sign-in failed: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}