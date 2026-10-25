using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Engine.Persistence;

namespace Triglav.Chess.Engine.Tests.LocalizationTests
{
    [TestClass]
    public class GuiResourceTests
    {
        private static readonly ResourceManager Manager = new("GuiLocalizationAudit", typeof(GuiResourceTests).Assembly);

        private static Dictionary<String, String> Catalogue(CultureInfo culture)
        {
            ResourceSet resources = Manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);
            Assert.IsNotNull(resources, $"Missing resource catalogue for {culture.Name}.");
            return resources.Cast<DictionaryEntry>().ToDictionary(static item => (String)item.Key, static item => (String)item.Value);
        }

        [TestMethod]
        public void EnglishAndPolishHaveIdenticalNonemptyKeysAndCompatiblePlaceholders()
        {
            Dictionary<String, String> english = Catalogue(CultureInfo.InvariantCulture);
            Dictionary<String, String> polish = Catalogue(CultureInfo.GetCultureInfo("pl"));
            CollectionAssert.AreEquivalent(english.Keys.ToArray(), polish.Keys.ToArray());
            foreach (String key in english.Keys)
            {
                Assert.IsFalse(String.IsNullOrWhiteSpace(english[key]), $"Empty English resource: {key}.");
                Assert.IsFalse(String.IsNullOrWhiteSpace(polish[key]), $"Empty Polish resource: {key}.");
                Assert.IsFalse(english[key].Contains('\uFFFD'), $"Invalid English encoding: {key}.");
                Assert.IsFalse(polish[key].Contains('\uFFFD'), $"Invalid Polish encoding: {key}.");
                String[] englishArguments = [.. Regex.Matches(english[key], @"\{\d+\}").Select(static match => match.Value)];
                String[] polishArguments = [.. Regex.Matches(polish[key], @"\{\d+\}").Select(static match => match.Value)];
                CollectionAssert.AreEquivalent(englishArguments, polishArguments, $"Mismatched format placeholders: {key}.");
                if (englishArguments.Length > 0)
                {
                    Int32 argumentCount = englishArguments.Max(static argument => Int32.Parse(argument[1..^1], CultureInfo.InvariantCulture)) + 1;
                    Object[] arguments = [.. Enumerable.Repeat<Object>("value", argumentCount)];
                    _ = String.Format(CultureInfo.GetCultureInfo("en"), english[key], arguments);
                    _ = String.Format(CultureInfo.GetCultureInfo("pl"), polish[key], arguments);
                }
            }
        }

        [TestMethod]
        public void EveryPgnDiagnosticHasEnglishAndPolishResources()
        {
            foreach (PgnDiagnosticCode code in Enum.GetValues<PgnDiagnosticCode>())
            {
                foreach (String language in new[] { "en", "pl" })
                {
                    Assert.IsFalse(String.IsNullOrEmpty(Manager.GetString("PgnError" + code, CultureInfo.GetCultureInfo(language))), $"Missing {language} diagnostic: {code}.");
                }
            }
        }

        [TestMethod]
        public void EveryNativeReplayErrorHasEnglishAndPolishResources()
        {
            foreach (GameReplayError code in Enum.GetValues<GameReplayError>())
            {
                foreach (String language in new[] { "en", "pl" })
                {
                    Assert.IsFalse(String.IsNullOrEmpty(Manager.GetString("GameReplay" + code, CultureInfo.GetCultureInfo(language))), $"Missing {language} replay diagnostic: {code}.");
                }
            }
        }

        [TestMethod]
        public void UnsupportedCultureFallsBackToEnglishAndPolishUsesSatellite()
        {
            Assert.AreEqual("Game finished", Manager.GetString("GameFinished", CultureInfo.GetCultureInfo("fr")));
            Assert.AreEqual("Partia zakończona", Manager.GetString("GameFinished", CultureInfo.GetCultureInfo("pl-PL")));
        }
    }
}
