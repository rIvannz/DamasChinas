using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DamasChinas_Client.UI.PopUps
{
    public partial class LoadingWindow : Window
    {
      
        private readonly Task _minDurationTask;

        public LoadingWindow()
        {
            InitializeComponent();

            
            TitleText.Text = MessageTranslator.GetLocalizedMessage("loadingTitle");

           
            _minDurationTask = Task.Delay(4000);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Loader.Resources["LoadingStoryboard"] is Storyboard storyboard)
                {
                    storyboard.Begin();
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[LoadingWindow.OnLoaded] {ex.Message}");
                
            }
        }

      
        public Task WaitMinimumAsync()
        {
            return _minDurationTask;
        }
    }
}













