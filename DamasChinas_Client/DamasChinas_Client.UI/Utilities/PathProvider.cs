using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PathProvider
    {
        private static readonly string BaseDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        private const string AvatarsFolder = "Assets/Avatars";


        private static readonly string[] AvatarFiles =
        {
            "avatar1.png",
            "avatar2.png",
            "avatar3.png",
            "avatar4.png",
            "avatar5.png"
        };


        public static string GetAssetPath(string relativePath)
        {
            return Path.Combine(BaseDirectory, relativePath);
        }


        public static Uri GetPackUri(string relativePackPath)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string uriStr = $"pack://application:,,,/{assemblyName};component/{relativePackPath}";
            return new Uri(uriStr, UriKind.Absolute);
        }
        public static string GetAvatarPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Avatar file name cannot be null or empty.", nameof(fileName));
            }

            string relative = Path.Combine(AvatarsFolder, fileName);
            return GetAssetPath(relative);
        }


        public static ImageSource LoadAvatar(string fileName)
        {
            string fullPath = GetAvatarPath(fileName);

            if (!File.Exists(fullPath))
            {
                
                fullPath = GetAvatarPath("avatar1.png");
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();

            return bmp;
        }


        public static string[] GetAvailableAvatarFiles()
        {
            return AvatarFiles;
        }
    }
}
