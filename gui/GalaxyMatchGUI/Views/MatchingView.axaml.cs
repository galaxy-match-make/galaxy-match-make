using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace GalaxyMatchGUI.Views
{
    public partial class MatchingView : UserControl
    {
        private Border? _profileCard;
        private const double SMALL_SCREEN_THRESHOLD = 500;
        private const double MEDIUM_SCREEN_THRESHOLD = 800;
        private const double LARGE_SCREEN_THRESHOLD = 1200;
        
        // Card width breakpoints
        private const double SMALL_CARD_WIDTH = 350;
        private const double MEDIUM_CARD_WIDTH = 450;
        private const double LARGE_CARD_WIDTH = 600;
        private const double XLARGE_CARD_WIDTH = 650;

        public MatchingView()
        {
            InitializeComponent();
            AttachedToVisualTree += MatchingView_AttachedToVisualTree;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _profileCard = this.FindControl<Border>("profileCard");
        }

        private void MatchingView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_profileCard == null)
            {
                _profileCard = this.FindControl<Border>("profileCard");
            }
            
            if (VisualRoot is Window window)
            {
                window.PropertyChanged += (s, e) => 
                {
                    if (e.Property == Window.BoundsProperty)
                    {
                        UpdateCardWidth(window.Bounds.Width);
                    }
                };
                
                UpdateCardWidth(window.Bounds.Width);
            }
        }

        private void UpdateCardWidth(double windowWidth)
        {
            if (_profileCard == null) return;

            if (windowWidth < SMALL_SCREEN_THRESHOLD)
            {
                _profileCard.MaxWidth = SMALL_CARD_WIDTH;
                _profileCard.Margin = new Thickness(10);
            }
            else if (windowWidth < MEDIUM_SCREEN_THRESHOLD)
            {
                _profileCard.MaxWidth = MEDIUM_CARD_WIDTH;
                _profileCard.Margin = new Thickness(15);
            }
            else if (windowWidth < LARGE_SCREEN_THRESHOLD)
            {
                _profileCard.MaxWidth = LARGE_CARD_WIDTH;
                _profileCard.Margin = new Thickness(20);
            }
            else
            {
                _profileCard.MaxWidth = XLARGE_CARD_WIDTH;
                _profileCard.Margin = new Thickness(20);
            }
        }
    }
}