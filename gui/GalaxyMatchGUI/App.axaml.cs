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
    

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {

            DisableAvaloniaDataAnnotationValidation();
            
            var services = new ServiceCollection();
            ConfigureServices(services);
            
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
            
            mainWindow.InitializeComponent();
            
            var navigationService = new NavigationService(() => mainWindow.GetContentArea ?? 
                throw new InvalidOperationException("ContentArea not found"));
            
            navigationService.RegisterView<LoginViewModel, LoginView>();
            navigationService.RegisterView<MatchingViewModel, MatchingView>();

            navigationService.RegisterView<InteractionsViewModel, InteractionsView>();

            navigationService.RegisterView<MessageRoomViewModel, MessageRoomView>();
            navigationService.RegisterView<ProfileViewModel, ProfileView>();
            navigationService.RegisterView<ViewOtherProfileViewModel, ViewOtherProfileView>();
            
            services.AddSingleton<INavigationService>(navigationService);
            
            ServiceProvider = services.BuildServiceProvider();
            

            navigationService.NavigateTo<LoginViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MatchingViewModel>();
        services.AddTransient<InteractionsViewModel>();
        services.AddTransient<MessageRoomViewModel>();
        services.AddTransient<ProfileViewModel>();
        
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}