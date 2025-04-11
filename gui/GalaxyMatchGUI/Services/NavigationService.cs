using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Threading;

namespace GalaxyMatchGUI.Services
{
    public class NavigationService
    {
        private Window? _mainWindow;

        public void Initialize(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void NavigateTo(Control newView)
        {
            if (_mainWindow is null)
                throw new InvalidOperationException("NavigationService is not initialized.");

            if (Dispatcher.UIThread.CheckAccess())
            {
                _mainWindow.Content = newView;
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _mainWindow.Content = newView;
                });
            }
        }

        public void NavigateToWindow(Window newWindow)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                PerformWindowNavigation(newWindow);
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    PerformWindowNavigation(newWindow);
                });
            }
        }

        private void PerformWindowNavigation(Window newWindow)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Close the current main window if it exists
                if (desktop.MainWindow != null)
                {
                    var oldWindow = desktop.MainWindow;
                    desktop.MainWindow = newWindow;
                    newWindow.Show();
                    oldWindow.Close();
                }
                else
                {
                    // If no main window exists, just set and show the new one
                    desktop.MainWindow = newWindow;
                    newWindow.Show();
                }
            }
        }
    }
}