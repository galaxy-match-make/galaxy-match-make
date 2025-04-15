using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GalaxyMatchGUI.ViewModels;
using GalaxyMatchGUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GalaxyMatchGUI.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            var navigationService = App.ServiceProvider?.GetService<INavigationService>();
            DataContext = new ProfileViewModel(navigationService);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}