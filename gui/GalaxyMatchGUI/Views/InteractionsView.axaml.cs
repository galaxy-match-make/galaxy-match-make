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
    private void OnReactionPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Contact contact)
        {
            contact.ReactionClickedCommand.Execute(null);
        }
    }
}