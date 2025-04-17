using CommunityToolkit.Mvvm.ComponentModel;

namespace GalaxyMatchGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Galaxy Match!";

    [ObservableProperty]
    private string _welcomeMessage = "Welcome, Explorer!";

    public MainWindowViewModel()
    {
    }
}
