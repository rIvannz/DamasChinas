using DamasChinas_Client;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DamasChinas_Client.UI.Utilities
{
    public static class AppNavigator
    {
        public static void NavigateToRoot(Page page)
        {
            if (page == null)
            {
                return;
            }

            try
            {
                var app = Application.Current;
                var shell = app?.MainWindow as AppShell;
                var frame = shell?.GetMainFrame();

                if (frame == null)
                {
                    return;
                }

                frame.Navigate(page);

                var nav = frame.NavigationService;
                if (nav == null)
                {
                    return;
                }

                while (nav.RemoveBackEntry() != null)
                {
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppNavigator.NavigateToRoot] {ex.Message}");
            }
        }

        public static void Navigate(Page page)
        {
            if (page == null)
            {
                return;
            }

            try
            {
                var app = Application.Current;
                var shell = app?.MainWindow as AppShell;
                var frame = shell?.GetMainFrame();

                frame?.Navigate(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppNavigator.Navigate] {ex.Message}");
            }
        }
    }
}
