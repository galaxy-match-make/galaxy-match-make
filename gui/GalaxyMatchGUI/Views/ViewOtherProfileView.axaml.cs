using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GalaxyMatchGUI.Views
{
    public partial class ViewOtherProfileView : UserControl
    {
        public ViewOtherProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}