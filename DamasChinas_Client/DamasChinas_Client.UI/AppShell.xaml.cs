using System;
using System.Windows.Controls;

namespace DamasChinas_Client
{
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        public Frame GetMainFrame()
        {
            return MainFrame;
        }
    }
}
