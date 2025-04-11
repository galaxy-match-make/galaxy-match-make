using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using GalaxyMatchGUI.ViewModels;
using GalaxyMatchGUI.Services;

namespace GalaxyMatchGUI
{
    public partial class App : Application
    {
        public static NavigationService NavigationService { get; } = new NavigationService();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            DataTemplates.Add(new ViewLocator());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                DisableAvaloniaDataAnnotationValidation();
                
                // Create the main window (LoginView) and initialize the NavigationService with it
                var mainWindow = new LoginView();
                NavigationService.Initialize(mainWindow);
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}