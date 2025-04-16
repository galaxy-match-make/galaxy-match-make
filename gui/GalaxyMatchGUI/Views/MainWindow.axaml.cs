using Avalonia.Controls;

namespace GalaxyMatchGUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public ContentControl? GetContentArea => this.FindControl<ContentControl>("ContentArea");
}