using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GalaxyMatchGUI.ViewModels;
using GalaxyMatchGUI.Views;
using GalaxyMatchGUI.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Avalonia.Media.Imaging;
using GalaxyMatchGUI.Models;

namespace GalaxyMatchGUI;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    public static AppSettings Settings { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // We'll handle SVG/image loading differently now
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            // Create main window first
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
            
            // Initialize window so ContentArea becomes available
            mainWindow.InitializeComponent();
            
            // Now pass a function that returns the ContentControl with null-safety
            var navigationService = new NavigationService(() => mainWindow.GetContentArea ?? 
                throw new InvalidOperationException("ContentArea not found"));
            
            // Register views with navigation service
            navigationService.RegisterView<LoginViewModel, LoginView>();
            navigationService.RegisterView<MatchingViewModel, MatchingView>();
            navigationService.RegisterView<InteractionsViewModel, InteractionsView>();
            navigationService.RegisterView<MessageRoomViewModel, MessageRoomView>();
            navigationService.RegisterView<ProfileViewModel, ProfileView>();
            
            // Register navigation service in DI container
            services.AddSingleton<INavigationService>(navigationService);
            
            // Build service provider after navigation service is registered
            ServiceProvider = services.BuildServiceProvider();
            
            // Navigate to login view
            navigationService.NavigateTo<LoginViewModel>();
            Settings = AppSettings.Load();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register view models
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MatchingViewModel>();
        services.AddTransient<InteractionsViewModel>();
        services.AddTransient<MessageRoomViewModel>();
        services.AddTransient<ProfileViewModel>();
        
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