using System;
using System.Collections.Generic;
using System.IO;

namespace DamasChinas_Server.Utilidades
{
    internal static class KeyValueFileLoader
    {
        public static IDictionary<string, string> Load(string relativePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"Configuration file not found: {fullPath}");
            }

            var values =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (string line in File.ReadAllLines(fullPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();

                values[key] = value;
            }

            return values;
        }
    }
}
