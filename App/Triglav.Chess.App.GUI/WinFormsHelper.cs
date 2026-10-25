using System;
using System.ComponentModel;
using System.Diagnostics;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI
{
    public static class WinFormsHelper
    {
        public const String TriglavGameFileExtension = ".TgGame";

        public static String TriglavGameFileExtensionFilter => $"{Resources.TriglavGameFilter}|*{TriglavGameFileExtension}";

        public const String PGNFileExtension = ".PGN";

        public static String PGNFileExtensionFilter => $"{Resources.PgnFilter}|*{PGNFileExtension}";

        public static String GameFileExtensionFilter => $"{Resources.GameFilesFilter}|*{TriglavGameFileExtension};*{PGNFileExtension}|{TriglavGameFileExtensionFilter}|{PGNFileExtensionFilter}";

        public const String TriglavPositionFileExtension = ".TgPosition";

        public static String TriglavPositionFileExtensionFilter => $"{Resources.TriglavPositionFilter}|*{TriglavPositionFileExtension}";

        public const String FENFileExtension = ".FEN";

        public static String FENFileExtensionFilter => $"{Resources.FenFilter}|*{FENFileExtension}";

        public static String PositionFileExtensionFilter => $"{TriglavPositionFileExtensionFilter}|{FENFileExtensionFilter}";

        public static Boolean IsInDesigner()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                || Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
        }
    }
}
