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
        // Tarea que representa el tiempo mínimo de visualización
        private readonly Task _minDurationTask;

        public LoadingWindow()
        {
            InitializeComponent();

            // Texto internacionalizado (loadingTitle en Lang.*.xaml)
            TitleText.Text = MessageTranslator.GetLocalizedMessage("loadingTitle");

            // 4 segundos como tiempo mínimo
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadingWindow.OnLoaded] {ex.Message}");
                // No re-lanzamos: el loader nunca debe tumbar la app
            }
        }

        /// <summary>
        /// Espera a que se cumpla el tiempo mínimo de visualización.
        /// </summary>
        public Task WaitMinimumAsync()
        {
            return _minDurationTask;
        }
    }
}













