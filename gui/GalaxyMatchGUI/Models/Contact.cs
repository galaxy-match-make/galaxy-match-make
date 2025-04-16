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

    [RelayCommand]
    public void AcceptRequest()
    {
        _interactionsViewModel?.AcceptRequestAsync(this);
    }
    
    [RelayCommand]
    public void RejectRequest()
    {
        _interactionsViewModel?.RejectRequestAsync(this);
    }
    
    [RelayCommand]
    public void CancelRequest()
    {
        _interactionsViewModel?.CancelRequestAsync(this);
    }
    
    [RelayCommand]
    private void ShowMessages()
    {
        _interactionsViewModel?.ShowMessages(this);
    }
    
    [RelayCommand]
    private void ShowProfile()
    {
        _interactionsViewModel?.ShowProfile(this);
    }
}