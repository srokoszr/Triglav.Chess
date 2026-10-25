using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Engine.Persistence;

namespace Triglav.Chess.App.GUI
{
    internal static class PositionFileDialogs
    {
        internal static Boolean IsFileError(Exception error) => error is GameReplayException or IOException or InvalidDataException or UnauthorizedAccessException or NotSupportedException or ArgumentException or ProtoBuf.ProtoException or PgnException;

        internal static void ShowError(IWin32Window owner, Exception error, String path = null)
        {
            Trace.TraceError(error.ToString());
            _ = MessageBox.Show(owner, UiText.FileErrorText(error, path), Resources.FileErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void Save(IWin32Window owner, Position position)
        {
            using SaveFileDialog dialog = new()
            {
                DefaultExt = "TgPosition",
                Filter = WinFormsHelper.PositionFileExtensionFilter,
                FileName = Resources.PositionFileName,
                Title = Resources.SavePositionTitle,
                AddExtension = true,
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                return;
            }
            try
            {
                PositionFile.Save(dialog.FileName, position);
            }
            catch (Exception error) when (IsFileError(error))
            {
                ShowError(owner, error, dialog.FileName);
            }
        }

        internal static Boolean TryLoad(IWin32Window owner, out Position position)
        {
            position = null;
            using OpenFileDialog dialog = new()
            {
                DefaultExt = "TgPosition",
                Filter = WinFormsHelper.PositionFileExtensionFilter,
                CheckFileExists = true,
                Title = Resources.LoadPositionTitle,
            };
            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                return false;
            }
            try
            {
                position = PositionFile.Load(dialog.FileName);
                return true;
            }
            catch (Exception error) when (IsFileError(error))
            {
                ShowError(owner, error, dialog.FileName);
                return false;
            }
        }
    }
}
