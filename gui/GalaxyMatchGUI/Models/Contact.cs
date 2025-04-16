using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.ViewModels;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Platform;

namespace GalaxyMatchGUI.Models;

public partial class Contact : ObservableObject
{
    InteractionsViewModel _interactionsViewModel;
    private Bitmap _avatarImage;
    private string _avatarUrl;
    public Contact(InteractionsViewModel interactionsViewModel)
    {
        _interactionsViewModel = interactionsViewModel;
    }

    public Contact(){}

    public void SetContactsViewModel(InteractionsViewModel interactionsViewModel)
    {
        _interactionsViewModel = interactionsViewModel;
    }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; }
    public string AvatarUrl
    {
        get => _avatarUrl;
        set
        {
            if (SetProperty(ref _avatarUrl, value))
            {
                // Load the image when the URL is set
                _ = LoadAvatarImageAsync();
            }
        }
    }

    public Bitmap AvatarImage
    {
        get => _avatarImage;
        private set => SetProperty(ref _avatarImage, value);
    }

    private async Task LoadAvatarImageAsync()
    {
        if (string.IsNullOrEmpty(AvatarUrl))
        {
            await LoadFallbackImage();
            return;
        }

        try
        {
            // Extract the base64 part after the comma
            int commaIndex = AvatarUrl.IndexOf(',');
            if (commaIndex > 0)
            {
                string data = AvatarUrl.Substring(commaIndex + 1);
                byte[] bytes = Convert.FromBase64String(data);
                
                using var stream = new MemoryStream(bytes);
                AvatarImage = new Bitmap(stream);
            }
            else
            {
                await LoadFallbackImage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading base64 image: {ex.Message}");
            await LoadFallbackImage();
        }
    }

    private async Task LoadFallbackImage()
    {
        try
        {
            var uri = new Uri("avares://GalaxyMatchGUI/Assets/alien_profile.png");
            using var stream = AssetLoader.Open(uri);
            AvatarImage = new Bitmap(stream);
        }
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"Error loading fallback image: {fallbackEx.Message}");
            AvatarImage = null;
        }
    }

    public async Task AcceptRequest()
    {
        await _interactionsViewModel?.AcceptRequestAsync(this);
    }
    
    public async Task RejectRequest()
    {
        await _interactionsViewModel?.RejectRequestAsync(this);
    }
    
    public async Task CancelRequest()
    {
        await _interactionsViewModel?.CancelRequestAsync(this);
    }
    
    [RelayCommand]
    private void ReactionClicked()
    {
        _interactionsViewModel?.HandleReactionClick(this);
    }
}