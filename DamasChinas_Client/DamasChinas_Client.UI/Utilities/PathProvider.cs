using System;
using System.IO;
using System.Reflection;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PathProvider
    {
        private static readonly string BaseDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        public static string GetAssetPath(string relativePath)
        {
            return Path.Combine(BaseDirectory, relativePath);
        }

        public static Uri GetPackUri(string relativePackPath)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            string uriStr =
                $"pack://application:,,,/{assemblyName};component/{relativePackPath}";

            return new Uri(uriStr, UriKind.Absolute);
        }
    }
}

