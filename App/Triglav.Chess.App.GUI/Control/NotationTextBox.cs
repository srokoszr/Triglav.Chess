using System;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI.Control
{
    internal static class NotationTextBox
    {
        internal static RichTextBox Create(String name, String accessibleName)
        {
            RichTextBox text = new()
            {
                Name = name,
                AccessibleName = accessibleName,
                ReadOnly = true,
                ShortcutsEnabled = true,
                DetectUrls = false,
                HideSelection = false,
                BackColor = SystemColors.Window,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };
            ContextMenuStrip menu = new();
            ToolStripItem copy = menu.Items.Add(Resources.CopyNotation, null, (_, _) => text.Copy());
            _ = menu.Items.Add(Resources.SelectAllNotation, null, (_, _) => text.SelectAll());
            menu.Opening += (_, _) => copy.Enabled = text.SelectionLength > 0;
            text.ContextMenuStrip = menu;
            text.Disposed += (_, _) => menu.Dispose();
            return text;
        }

        internal static void Update(RichTextBox text, String value)
        {
            if (text.Text == value)
            {
                return;
            }
            Int32 start = text.SelectionStart;
            Int32 length = text.SelectionLength;
            Boolean followEnd = length == 0 && start == text.TextLength;
            text.Text = value;
            start = Math.Min(start, text.TextLength);
            text.Select(followEnd ? text.TextLength : start, Math.Min(length, text.TextLength - start));
            if (followEnd)
            {
                text.ScrollToCaret();
            }
        }
    }
}
