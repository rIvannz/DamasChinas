using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PathProvider
    {
        private const string AvatarsFolder = "Assets/Avatars/";
        private const string DefaultAvatarFile = "avatarIcon.png";

        public static Uri GetPackUri(string relativePackPath)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string sanitized = relativePackPath.Replace("\\", "/");
            string uriStr = $"pack://application:,,,/{assemblyName};component/{sanitized}";
            return new Uri(uriStr, UriKind.Absolute);
        }

        public static string GetAvatarPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = DefaultAvatarFile;
            }

            string relative = $"{AvatarsFolder}{fileName}";
            return GetPackUri(relative).ToString();
        }

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
            catch
            {
                try
                {
                    var fallback = new Uri(GetAvatarPath(DefaultAvatarFile), UriKind.Absolute);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = fallback;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    return bmp;
                }
                catch
                {
                    return null;
                }
            }
        }

        // ============================================================
        // USADO POR SelectAvatar → corrige CS0117
        // ============================================================
        public static IEnumerable<string> GetAvailableAvatarFiles()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // AvatarsFolder viene con "/" → lo adaptamos al separador del SO
                string avatarsPath = AvatarsFolder.Replace(
                    '/',
                    Path.DirectorySeparatorChar);

                string avatarsDir = Path.Combine(baseDir, avatarsPath);

                if (!Directory.Exists(avatarsDir))
                {
                    return new[] { DefaultAvatarFile };
                }

                var files = Directory
                    .EnumerateFiles(avatarsDir, "*.png")
                    .Select(Path.GetFileName)
                    .ToList();

                if (!files.Any())
                {
                    files.Add(DefaultAvatarFile);
                }

                return files;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PathProvider.GetAvailableAvatarFiles] {ex.Message}");
                return new[] { DefaultAvatarFile };
            }
        }
    }
}
