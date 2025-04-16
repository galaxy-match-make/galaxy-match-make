using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GalaxyMatchGUI.Services;
using GalaxyMatchGUI.ViewModels;
namespace GalaxyMatchGUI.Views;

public partial class ContactsView : UserControl
{
    public ContactsView()
    {
        InitializeComponent();
        DataContext = new ContactsViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}