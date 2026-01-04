using DamasChinas_Shared.Contracts.Dtos;

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DamasChinas_Client.UI.Utilities
{
    public static class PendingBanNotificationStore
    {
        private const string FileName = "pending_ban_notification.json";

        public static void Save(BanInfoDto banInfo)
        {
            if (banInfo == null)
            {
                return;
            }
            try
            {
                string path = GetFilePath();

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var serializer = new DataContractJsonSerializer(typeof(BanInfoDto));
                    serializer.WriteObject(fs, banInfo);
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
            string appFolder = Path.Combine(folder, "DamasChinas");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            return Path.Combine(appFolder, FileName);
        }

        public static string BuildBanMessage(BanInfoDto banInfo)
        {
            if (banInfo == null)
            {
                return string.Empty;
            }
            if (banInfo.IsPermanent)
            {
                return MessageTranslator.GetLocalizedMessage(MessageKeys.UserBannedPermanent);
            }
            if (banInfo.BanUntilUtc.HasValue)
            {
                DateTime local = banInfo.BanUntilUtc.Value.ToLocalTime();
                string until = local.ToString("dd/MM/yyyy HH:mm");

                string template =
                    MessageTranslator.GetLocalizedMessage(MessageKeys.UserBannedUntil);

                return string.Format(template, until);
            }

            return MessageTranslator.GetLocalizedMessage(MessageKeys.UserBanned);
        }

    }
}
