using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PathProvider
    {
        // ============================================================
        // BASE (por si en algún punto necesitas rutas físicas)
        // ============================================================
        private static readonly string BaseDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Carpeta lógica dentro del proyecto/assembly
        private const string AvatarsFolder = "Assets/Avatars";
        private const string DefaultAvatarFile = "avatar1.png";

        // Lista de avatares disponibles
        private static readonly string[] AvatarFiles =
        {
            "avatar1.png",
            "avatar2.png",
            "avatar3.png",
            "avatar4.png",
            "avatar5.png"
        };

        // ============================================================
        // utilidades generales
        // ============================================================
        public static string GetAssetPath(string relativePath)
        {
            return Path.Combine(BaseDirectory, relativePath);
        }

        public static Uri GetPackUri(string relativePackPath)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string sanitized = relativePackPath.Replace("\\", "/");
            string uriStr = $"pack://application:,,,/{assemblyName};component/{sanitized}";
            return new Uri(uriStr, UriKind.Absolute);
        }

        // ============================================================
        // AVATARES
        // ============================================================

        // Devuelve un string con la URI pack://... (sirve para el binding en SelectAvatar)
        public static string GetAvatarPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = DefaultAvatarFile;
            }

            string relative = $"{AvatarsFolder}/{fileName}";
            return GetPackUri(relative).ToString();
        }

        // Carga el avatar como ImageSource (usado en ProfilePlayer)
        public static ImageSource LoadAvatar(string fileName)
        {
            try
            {
                var uri = new Uri(GetAvatarPath(fileName), UriKind.Absolute);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                return bmp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PathProvider.LoadAvatar] Error loading avatar '{fileName}': {ex.Message}");

                // Fallback a avatar por defecto
                try
                {
                    var fallbackUri = new Uri(GetAvatarPath(DefaultAvatarFile), UriKind.Absolute);

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = fallbackUri;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    return bmp;
                }
                catch (Exception innerEx)
                {
                    Debug.WriteLine($"[PathProvider.LoadAvatar] Error loading default avatar: {innerEx.Message}");
                    return null;
                }
            }
        }

        public static string[] GetAvailableAvatarFiles()
        {
            return AvatarFiles;
        }
    }
}
