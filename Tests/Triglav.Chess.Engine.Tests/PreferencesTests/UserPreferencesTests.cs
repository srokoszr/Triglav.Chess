using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Triglav.Chess.App.GUI.Model;

namespace Triglav.Chess.Engine.Tests.PreferencesTests
{
    [TestClass]
    public class UserPreferencesTests
    {
        private String directory;
        private String settingsPath;

        [TestInitialize]
        public void Initialize()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "TriglavPreferences-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(this.directory);
            this.settingsPath = Path.Combine(this.directory, "settings.json");
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(this.directory, recursive: true);

        [TestMethod]
        public void MissingSettingsUseDocumentsDefaultWithoutWritingUntilSaved()
        {
            UserPreferences preferences = UserPreferencesFile.Load(this.settingsPath);
            Assert.AreEqual(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Triglav Chess", "Games"), preferences.GameSaveDirectory);
            Assert.IsFalse(File.Exists(this.settingsPath));
        }

        [TestMethod]
        public void SavedFolderSurvivesReloadAndCreatesUniqueNativeGamePaths()
        {
            String folder = Path.Combine(this.directory, "Partie łódzkie");
            UserPreferencesFile.Save(new UserPreferences { GameSaveDirectory = folder }, this.settingsPath);
            UserPreferences reloaded = UserPreferencesFile.Load(this.settingsPath);
            Assert.AreEqual(folder, reloaded.GameSaveDirectory);
            String first = reloaded.CreateGameFilePath();
            String second = reloaded.CreateGameFilePath();
            Assert.AreEqual(folder, Path.GetDirectoryName(first));
            Assert.AreEqual(".TgGame", Path.GetExtension(first));
            Assert.AreNotEqual(first, second);
            Assert.IsFalse(File.Exists(first));
        }

        [TestMethod]
        public void SavingChangedFolderReplacesPreferenceAndLeavesNoTemporaryFiles()
        {
            UserPreferencesFile.Save(new UserPreferences { GameSaveDirectory = this.directory }, this.settingsPath);
            String changed = Path.Combine(this.directory, "Games");
            UserPreferencesFile.Save(new UserPreferences { GameSaveDirectory = changed }, this.settingsPath);
            Assert.AreEqual(changed, UserPreferencesFile.Load(this.settingsPath).GameSaveDirectory);
            Assert.HasCount(1, Directory.GetFiles(this.directory));
        }

        [TestMethod]
        public void InvalidPreferenceDoesNotReplacePreviouslySavedSettings()
        {
            UserPreferencesFile.Save(new UserPreferences { GameSaveDirectory = this.directory }, this.settingsPath);
            String original = File.ReadAllText(this.settingsPath);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => UserPreferencesFile.Save(new UserPreferences { GameSaveDirectory = "relative-folder" }, this.settingsPath));
            Assert.AreEqual(original, File.ReadAllText(this.settingsPath));
        }

        [TestMethod]
        [DataRow("{broken")]
        [DataRow("null")]
        [DataRow("{\"GameSaveDirectory\":null}")]
        [DataRow("{\"GameSaveDirectory\":\"relative\"}")]
        public void MalformedSettingsAreReportedAndPreserved(String content)
        {
            File.WriteAllText(this.settingsPath, content);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => UserPreferencesFile.Load(this.settingsPath));
            Assert.AreEqual(content, File.ReadAllText(this.settingsPath));
        }
    }
}
