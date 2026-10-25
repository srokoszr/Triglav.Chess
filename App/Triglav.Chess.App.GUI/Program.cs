using System;
using System.Windows.Forms;
using Triglav.Chess.Engine.Core;

namespace Triglav.Chess.App.GUI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            ZobristTable.Init();
            Application.Run(new MainForm());
        }
    }
}