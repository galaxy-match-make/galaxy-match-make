using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GalaxyMatchGUI.Models;
using GalaxyMatchGUI.Services;
using GalaxyMatchGUI.ViewModels;
namespace GalaxyMatchGUI.Views;

public partial class InteractionsView : UserControl
{
    public InteractionsView()
    {
        InitializeComponent();
        DataContext = new InteractionsViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private void OnShowMessagesPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Contact contact)
        {
            contact.ShowMessagesCommand.Execute(null);
        }
    }
    
    private void OnShowProfilePressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Contact contact)
        {
            contact.ShowProfileCommand.Execute(null);
        }
    }
}