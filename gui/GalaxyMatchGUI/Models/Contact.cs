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