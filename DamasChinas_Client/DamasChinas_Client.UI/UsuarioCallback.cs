using DamasChinas_Client.UI.UsuarioServiceReference;
using System;
using System.Windows;

public class UsuarioCallback : IUsuarioServiceCallback
{
    public void UsuarioCreado(string mensaje)
    {
        // Mostrar un MessageBox directamente
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show("✅ Usuario creado: " + mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    public void ErrorCreandoUsuario(string mensaje)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show("❌ Error creando usuario: " + mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
    }
}
