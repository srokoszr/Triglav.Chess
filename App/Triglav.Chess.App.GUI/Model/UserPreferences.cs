using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Triglav.Chess.Engine.Persistence;

namespace Triglav.Chess.App.GUI.Model
{
    public sealed class UserPreferences
    {
        public String GameSaveDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Triglav Chess", "Games");

        public String CreateGameFilePath()
        {
            ValidateDirectory(this.GameSaveDirectory);
            return Path.Combine(this.GameSaveDirectory, $"game-{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}-{Guid.NewGuid():N}{GameFile.Extension}");
        }

        internal static void ValidateDirectory(String directory)
        {
            if (String.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory))
            {
                throw new InvalidDataException("The game save directory must be an absolute path.");
            }
            _ = Path.GetFullPath(directory);
        }
    }

    public static class UserPreferencesFile
    {
        public static String FilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Triglav Chess", "settings.json");

        public static UserPreferences Load(String path = null)
        {
            path ??= FilePath;
            if (!File.Exists(path))
            {
                return new UserPreferences();
            }
            try
            {
                UserPreferences preferences = JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(path))
                    ?? throw new InvalidDataException("The settings file is empty.");
                UserPreferences.ValidateDirectory(preferences.GameSaveDirectory);
                return preferences;
            }
            catch (JsonException error)
            {
                throw new InvalidDataException("The settings file is malformed.", error);
            }
        }

        public static void Save(UserPreferences preferences, String path = null)
        {
            ArgumentNullException.ThrowIfNull(preferences);
            UserPreferences.ValidateDirectory(preferences.GameSaveDirectory);
            path = Path.GetFullPath(path ?? FilePath);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
            AtomicFile.Write(path, stream => JsonSerializer.Serialize(stream, preferences, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
