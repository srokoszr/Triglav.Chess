using System;
using System.IO;

namespace Triglav.Chess.Engine.Persistence
{
    public static class AtomicFile
    {
        public static void Write(String path, Action<Stream> write)
        {
            ArgumentNullException.ThrowIfNull(write);
            String destination = Path.GetFullPath(path);
            String temporary = Path.Combine(Path.GetDirectoryName(destination), $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
            try
            {
                using (FileStream stream = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    write(stream);
                    stream.Flush(flushToDisk: true);
                }
                File.Move(temporary, destination, overwrite: true);
            }
            finally
            {
                try
                {
                    File.Delete(temporary);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
