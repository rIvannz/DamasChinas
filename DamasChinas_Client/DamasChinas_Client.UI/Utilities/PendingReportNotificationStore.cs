using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PendingReportNotificationStore
    {
        private const string FileName = "pending_report_notification.json";
        private const string Nombre = "DamasChinas";

        public static void Save(BanInfoDto info)
        {
            if (info == null)
            {
                return;
            }
            try
            {
                string path = GetFilePath();

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var serializer = new DataContractJsonSerializer(typeof(BanInfoDto));
                    serializer.WriteObject(fs, info);
                }
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);

            }
        }

        public static BanInfoDto Load()
        {
            try
            {
                string path = GetFilePath();

                if (!File.Exists(path))
                {
                    return null;
                }
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var serializer = new DataContractJsonSerializer(typeof(BanInfoDto));
                    return serializer.ReadObject(fs) as BanInfoDto;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void Clear()
        {
            try
            {
                string path = GetFilePath();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private static string GetFilePath()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(folder,Nombre);

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            return Path.Combine(appFolder, FileName);
        }

        public static string BuildMessage(BanInfoDto info)
        {
            if (info == null)
            {
                return string.Empty;
            }
            string baseMsg = MessageTranslator.GetLocalizedMessage(MessageKeys.PlayerReported);

            if (info.TotalReports > 0)
            {
                return $"{baseMsg} ({info.TotalReports})";
            }
            return baseMsg;
        }
    }
}
