using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
	public static class MessageHelper
	{
		public static void ShowSuccess(string message, string title = "Éxito")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void ShowError(string message, string title = "Error")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void ShowWarning(string message, string title = "Advertencia")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		public static void ShowInfo(string message, string title = "Información")
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void ShowFromResult(SingInServiceProxy.OperationResult result)
		{
			if (result == null)
			{
				ShowError("Ocurrió un error inesperado.");
				return;
			}

			if (result.Succes)
			{
				ShowSuccess(result.Messaje);
			}
			else
			{
				ShowError(result.Messaje);
			}
		}
	}
}
