using System;
using System.IO;
using ShellLink;

namespace Affinity_manager.Model.DataGathering
{
    public static class ShortcutExtensions
    {
        public static string? GetExeTargetFullPath(this Shortcut shortcut)
        {
            string? targetPath = shortcut.LinkTargetIDList?.Path;
            if (targetPath == null)
            {
                targetPath = shortcut.ExtraData?.EnvironmentVariableDataBlock?.TargetUnicode;
            }

            if (string.IsNullOrEmpty(targetPath) || !Path.GetExtension(targetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(RemoveParameters(targetPath)));
        }

        public static string? GetIconPath(this Shortcut shortcut)
        {
            string? iconPath = shortcut.StringData?.IconLocation != null ? Environment.ExpandEnvironmentVariables(shortcut.StringData.IconLocation) : null;
            if (iconPath != null && Path.GetExtension(iconPath).Equals(".ico", StringComparison.OrdinalIgnoreCase) && Path.Exists(iconPath))
            {
                return iconPath;
            }

            return shortcut.GetExeTargetFullPath();
        }

        private static string RemoveParameters(string targetPath)
        {
            int indexOfDotExe = targetPath.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            return targetPath.Substring(0, indexOfDotExe + 4);
        }
    }
}
