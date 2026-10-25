using System.IO;
using System;

namespace Triglav.Chess.Engine.Benchmarks
{
    public class PathHelper
    {
        public static string CurrentLocation => CurrentLocationDirectoryInfo.FullName;

        public static DirectoryInfo CurrentLocationDirectoryInfo => new DirectoryInfo(AppContext.BaseDirectory);
    }
}
