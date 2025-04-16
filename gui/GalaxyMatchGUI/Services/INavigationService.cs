using GalaxyMatchGUI.ViewModels;

namespace GalaxyMatchGUI.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>(T? viewModel = null) where T : ViewModelBase;

        bool NavigateBack();
    }
}