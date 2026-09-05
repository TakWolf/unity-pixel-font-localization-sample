using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PixelFontLocalization.Editor.Fonts
{
    public static class ProjectFontAssetGenerationMenu
    {
        private const string TableCollectionName = "Sample Text";
        private const string SourceRoot = "Assets/PixelFontLocalization/Fonts/fusion-pixel-12px";
        private const string OutputRoot = "Assets/PixelFontLocalization/Fonts/FontAssets";

        private static void Generate(string sourceFileName, string outputFileName, IReadOnlyCollection<string> localeCodes)
        {
            var localizedCharacters = LocalizationCharacterSetBuilder.Build(new[] { TableCollectionName }, localeCodes);
            var initialCharacterSet = CharacterSetBuilder.Combine(CharacterSetBuilder.TextMeshProDefaults(), localizedCharacters);
            new FontAssetGenerator(
                $"{SourceRoot}/{sourceFileName}",
                $"{OutputRoot}/{outputFileName}",
                12,
                FontRenderingStyle.Pixel,
                FontAssetPopulationMode.Dynamic,
                initialCharacterSet).Generate();
        }

        [MenuItem("Tools/Pixel Font Localization/Generate Font Assets")]
        public static void GenerateFontAssets()
        {
            Generate(
                "fusion-pixel-12px-proportional-latin.otf",
                "Pixel-12-Latin.asset",
                new[]
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
                    "el"
                });

            Generate(
                "fusion-pixel-12px-proportional-zh_hans.otf",
                "Pixel-12-SimplifiedChinese.asset",
                new[] { "zh-Hans" });

            Generate(
                "fusion-pixel-12px-proportional-zh_hant.otf",
                "Pixel-12-TraditionalChinese.asset",
                new[] { "zh-Hant" });

            Generate(
                "fusion-pixel-12px-proportional-ja.otf",
                "Pixel-12-Japanese.asset",
                new[] { "ja" });

            Generate(
                "fusion-pixel-12px-proportional-ko.otf",
                "Pixel-12-Korean.asset",
                new[] { "ko" });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Font asset generation completed.");
        }
    }
}
