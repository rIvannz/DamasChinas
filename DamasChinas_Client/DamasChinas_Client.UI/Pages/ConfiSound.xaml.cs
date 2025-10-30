using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
	public partial class ConfiSound : Page
	{
		private double _pendingVolume;

		public ConfiSound()
		{
			InitializeComponent();

			_pendingVolume = SoundManager.MusicVolume;
			MusicSlider.Value = _pendingVolume * 100;

			MusicSlider.ValueChanged += OnMusicVolumeChanged;
		}

		private void OnMusicVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_pendingVolume = e.NewValue / 100;
			SoundManager.ApplyVolume(_pendingVolume);
		}

		private void OnConfirmClick(object sender, RoutedEventArgs e)
		{
			SoundManager.ApplyVolume(_pendingVolume);
			MessageBox.Show(
				"Sound settings updated successfully.",
				"Sound",
				MessageBoxButton.OK,
				MessageBoxImage.Information);
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			if (NavigationService?.CanGoBack == true)
			{
				NavigationService.GoBack();
			}
		}
	}
}
