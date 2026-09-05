using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace PixelFontLocalization.Tests.Editor.Localization
{
    public sealed class LocalizationResourceTests
    {
        private static readonly string[] LocaleCodes =
        {
            "en",
            "fr",
            "it",
            "de",
            "es",
            "pt-BR",
            "pl",
            "tr",
            "ru",
            "el",
            "zh-Hans",
            "zh-Hant",
            "ja",
            "ko"
        };

        private static readonly string[] StringKeys =
        {
            "sample.title",
            "sample.language-name",
            "sample.content",
            "ui.language",
            "ui.close",
            "ui.input-placeholder"
        };

        [Test]
        public void SampleTextCollection_ContainsEveryRequiredLocaleAndEntry()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("Sample Text");

            Assert.That(collection, Is.Not.Null);

            foreach (var localeCode in LocaleCodes)
            {
                var table = collection.GetTable(new LocaleIdentifier(localeCode)) as StringTable;
                Assert.That(table, Is.Not.Null, $"Missing Sample Text table for {localeCode}.");

                foreach (var key in StringKeys)
                {
                    var entry = table.GetEntry(key);
                    Assert.That(entry, Is.Not.Null, $"Missing {key} in Sample Text table for {localeCode}.");
                    Assert.That(entry.LocalizedValue, Is.Not.Empty, $"Empty {key} in Sample Text table for {localeCode}.");
                }
            }
        }

        [Test]
        public void FontCollection_ContainsValidFontForEveryRequiredLocale()
        {
            var collection = LocalizationEditorSettings.GetAssetTableCollection("Fonts");

            Assert.That(collection, Is.Not.Null);

            foreach (var localeCode in LocaleCodes)
            {
                var table = collection.GetTable(new LocaleIdentifier(localeCode)) as AssetTable;
                Assert.That(table, Is.Not.Null, $"Missing Fonts table for {localeCode}.");

                var entry = table.GetEntry("font.default");
                Assert.That(entry, Is.Not.Null, $"Missing font.default in Fonts table for {localeCode}.");
                Assert.That(entry.Guid, Is.Not.Empty, $"Empty font.default in Fonts table for {localeCode}.");

                var assetPath = AssetDatabase.GUIDToAssetPath(entry.Guid);
                var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
                Assert.That(fontAsset, Is.Not.Null, $"font.default for {localeCode} does not reference a TMP Font Asset.");
                Assert.That(fontAsset.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic), $"Font for {localeCode} must be Dynamic.");
                Assert.That(fontAsset.sourceFontFile, Is.Not.Null, $"Dynamic font for {localeCode} must retain its source font.");
                Assert.That(fontAsset.isMultiAtlasTexturesEnabled, Is.True, $"Font for {localeCode} must enable Multi Atlas Textures.");

                var serializedFontAsset = new SerializedObject(fontAsset);
                var clearDynamicDataOnBuild = serializedFontAsset.FindProperty("m_ClearDynamicDataOnBuild");
                Assert.That(clearDynamicDataOnBuild, Is.Not.Null);
                Assert.That(clearDynamicDataOnBuild.boolValue, Is.False, $"Font for {localeCode} must retain prewarmed data in builds.");
            }
        }

        [Test]
        public void FontCollection_UsesExpectedSharedFontGroups()
        {
            var expectedFontNames = new Dictionary<string, string>
            {
                ["en"] = "Pixel-12-Latin",
                ["fr"] = "Pixel-12-Latin",
                ["it"] = "Pixel-12-Latin",
                ["de"] = "Pixel-12-Latin",
                ["es"] = "Pixel-12-Latin",
                ["pt-BR"] = "Pixel-12-Latin",
                ["pl"] = "Pixel-12-Latin",
                ["tr"] = "Pixel-12-Latin",
                ["ru"] = "Pixel-12-Latin",
                ["el"] = "Pixel-12-Latin",
                ["zh-Hans"] = "Pixel-12-SimplifiedChinese",
                ["zh-Hant"] = "Pixel-12-TraditionalChinese",
                ["ja"] = "Pixel-12-Japanese",
                ["ko"] = "Pixel-12-Korean"
            };

            var collection = LocalizationEditorSettings.GetAssetTableCollection("Fonts");

            foreach (var pair in expectedFontNames)
            {
                var table = collection.GetTable(new LocaleIdentifier(pair.Key)) as AssetTable;
                Assert.IsNotNull(table);

                var entry = table.GetEntry("font.default");
                var assetPath = AssetDatabase.GUIDToAssetPath(entry.Guid);
                var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);

                Assert.That(fontAsset.name, Is.EqualTo(pair.Value), $"Unexpected font mapping for {pair.Key}.");
            }
        }
    }
}
