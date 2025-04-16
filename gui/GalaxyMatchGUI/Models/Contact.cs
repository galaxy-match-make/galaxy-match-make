using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.ViewModels;

namespace GalaxyMatchGUI.Models;

public partial class Contact
{
    InteractionsViewModel _interactionsViewModel;
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
    public string AvatarUrl { get; set; }

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