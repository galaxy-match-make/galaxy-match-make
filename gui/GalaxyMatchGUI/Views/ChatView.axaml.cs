using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyMatchGUI.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyMatchGUI.Views
{
    public partial class ChatView : UserControl
    {
       
        public ChatView()
        {
            InitializeComponent();
        
            DataContext = new ChatViewModel();

            

           
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

       
    }
}
