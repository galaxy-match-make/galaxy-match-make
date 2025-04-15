using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyMatchGUI.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Input;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI.ViewModels
{
    public partial class InteractionsViewModel : ViewModelBase
    {
        [ObservableProperty] private ObservableCollection<Contact> _messageContacts = new();
        [ObservableProperty] private bool _isLoadingMessages;
        [ObservableProperty] private string _messagesStatusMessage = string.Empty;
        
        [ObservableProperty] private ObservableCollection<Contact> _sentRequestContacts = new();
        [ObservableProperty] private bool _isLoadingSentRequests;
        [ObservableProperty] private string _sentRequestsStatusMessage = string.Empty;
        
        [ObservableProperty] private ObservableCollection<Contact> _receivedRequestContacts = new();
        [ObservableProperty] private bool _isLoadingReceivedRequests;
        [ObservableProperty] private string _receivedRequestsStatusMessage = string.Empty;
        
        [ObservableProperty] private bool _showEmptyState;
        [ObservableProperty] private string _emptyStateMessage = "No connections found. Start exploring the cosmos!";

        [ObservableProperty] private Contact _selectedContact;
        
        public InteractionsViewModel()
        {
            Task.Run(async () => {
                await LoadMessagesAsync();
                await LoadSentRequestsAsync();
                await LoadReceivedRequestsAsync();
            });
        }

        #region Messages Tab

        [RelayCommand]
        private async Task RefreshMessagesAsync()
        {
            await LoadMessagesAsync();
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                IsLoadingMessages = true;
                MessagesStatusMessage = "Loading message contacts...";

                MessageContacts.Clear();

                var messages = await GetMessageContactsAsync();
                foreach (var contact in messages)
                {
                    MessageContacts.Add(contact);
                }

                MessagesStatusMessage = MessageContacts.Count > 0 
                    ? $"Found {MessageContacts.Count} message contacts" 
                    : "";
                
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                MessagesStatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoadingMessages = false;
            }
        }

        private async Task<ObservableCollection<Contact>> GetMessageContactsAsync()
        {
            var httpClient = HttpService.Instance;
            var reactionsList = (await httpClient.GetJsonAsync<List<Contact>>("/api/interactions", true)) ?? new List<Contact>();
            foreach (var contact in reactionsList)
            {
                contact.SetContactsViewModel(this);
            }

            return new ObservableCollection<Contact>(reactionsList);
        }




        #endregion

        #region Sent Requests Tab

        [RelayCommand]
        private async Task RefreshSentRequestsAsync()
        {
            await LoadSentRequestsAsync();
        }

        private async Task LoadSentRequestsAsync()
        {
            try
            {
                IsLoadingSentRequests = true;
                SentRequestsStatusMessage = "Loading sent requests...";

                SentRequestContacts.Clear();
                
                var sentRequests = await GetSentRequestContacts();
                foreach (var contact in sentRequests)
                {
                    SentRequestContacts.Add(contact);
                }

                SentRequestsStatusMessage = SentRequestContacts.Count > 0 
                    ? $"You have {SentRequestContacts.Count} pending requests" 
                    : "No pending requests";
                
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                SentRequestsStatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoadingSentRequests = false;
            }
        }

        private async Task<ObservableCollection<Contact>> GetSentRequestContacts()
        {
            var httpClient = HttpService.Instance;
            var reactionsList = (await httpClient.GetJsonAsync<List<Contact>>("/api/interactions/sent-requests", true)) ?? new List<Contact>();
            foreach (var reaction in reactionsList)
            {
                reaction.SetContactsViewModel(this);
            }
            return new ObservableCollection<Contact>(reactionsList);
        }
        
        public async Task CancelRequestAsync(Contact reaction)
        {
            try
            {
                if (reaction == null) return;
                
                IsLoadingSentRequests = true;
                SentRequestsStatusMessage = $"Canceling request to {reaction.DisplayName}...";
                
                SentRequestContacts.Remove(reaction);
                
                SentRequestsStatusMessage = $"Request to {reaction.DisplayName} canceled";
                
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                SentRequestsStatusMessage = $"Error canceling request: {ex.Message}";
            }
            finally
            {
                IsLoadingSentRequests = false;
            }
        }

        #endregion

        #region Received Requests Tab

        [RelayCommand]
        private async Task RefreshReceivedRequestsAsync()
        {
            await LoadReceivedRequestsAsync();
        }

        private async Task LoadReceivedRequestsAsync()
        {
            try
            {
                IsLoadingReceivedRequests = true;
                ReceivedRequestsStatusMessage = "Loading received requests...";

                ReceivedRequestContacts.Clear();
                
                var receivedRequests = await GetReceivedRequestContacts();
                foreach (var contact in receivedRequests)
                {
                    ReceivedRequestContacts.Add(contact);
                }

                ReceivedRequestsStatusMessage = ReceivedRequestContacts.Count > 0 
                    ? $"You have {ReceivedRequestContacts.Count} reaction requests" 
                    : "No reaction requests";
                
                UpdateEmptyState();
            }
            catch (Exception ex)
            {
                ReceivedRequestsStatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoadingReceivedRequests = false;
            }
        }

        private async Task<ObservableCollection<Contact>> GetReceivedRequestContacts()
        {
            var httpClient = HttpService.Instance;
            var reactionsList = (await httpClient.GetJsonAsync<List<Contact>>("/api/interactions/received-requests", true)) ?? new List<Contact>();
            foreach (var contact in reactionsList)
            {
                contact.SetContactsViewModel(this);
            }
            return new ObservableCollection<Contact>(reactionsList);
        }
        
        public async Task AcceptRequestAsync(Contact reaction)
        {
            try
            {
                if (reaction == null) return;
                
                IsLoadingReceivedRequests = true;
                ReceivedRequestsStatusMessage = $"Accepting request from {reaction.DisplayName}...";
                
                ReceivedRequestContacts.Remove(reaction);
                MessageContacts.Add(reaction);
                
                ReceivedRequestsStatusMessage = $"Connected with {reaction.DisplayName}!";
                
                UpdateEmptyState();
                MessagesStatusMessage = $"Found {MessageContacts.Count} message reactions";
            }
            catch (Exception ex)
            {
                ReceivedRequestsStatusMessage = $"Error accepting request: {ex.Message}";
            }
            finally
            {
                IsLoadingReceivedRequests = false;
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            NavigationService?.NavigateTo<MatchingViewModel>();
        }

        #endregion

        private void UpdateEmptyState()
        {
            bool allEmpty = MessageContacts.Count == 0 && 
                           SentRequestContacts.Count == 0 && 
                           ReceivedRequestContacts.Count == 0;
            
            ShowEmptyState = allEmpty;
            
            if (allEmpty)
            {
                EmptyStateMessage = "Your cosmic network is empty.\nStart exploring to connect with fellow space travelers!";
            }
        }
        
        public void HandleReactionClick(Contact contact)
        {
            SelectedContact = contact;
            Console.WriteLine($"Navigating to profile: {contact.DisplayName}");
            NavigationService?.NavigateTo(new MessageRoomViewModel(contact));
        }
    }
}