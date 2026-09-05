using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace PixelFontLocalization.Editor.Fonts
{
    public enum FontRenderingStyle
    {
        Standard,
        Pixel
    }

    public enum FontAssetPopulationMode
    {
        Static,
        Dynamic
    }

    public sealed class FontAssetGenerator
    {
        private static int GetDefaultAtlasPadding(FontRenderingStyle renderingStyle) => renderingStyle == FontRenderingStyle.Pixel ? 1 : 9;

        private readonly string _sourceFontPath;
        private readonly string _outputAssetPath;
        private readonly int _samplingPointSize;
        private readonly FontRenderingStyle _renderingStyle;
        private readonly FontAssetPopulationMode _populationMode;
        private readonly uint[] _initialCharacterSet;
        private readonly int _atlasSize;
        private readonly int _atlasPadding;
        private readonly bool _enableMultiAtlas;

        public FontAssetGenerator(
            string sourceFontPath,
            string outputAssetPath,
            int samplingPointSize,
            FontRenderingStyle renderingStyle,
            FontAssetPopulationMode populationMode,
            IReadOnlyCollection<uint> initialCharacterSet = null,
            int atlasSize = 1024,
            int? atlasPadding = null,
            bool? enableMultiAtlas = null)
        {
            _sourceFontPath = sourceFontPath;
            _outputAssetPath = outputAssetPath;
            _samplingPointSize = samplingPointSize;
            _renderingStyle = renderingStyle;
            _populationMode = populationMode;
            _initialCharacterSet = initialCharacterSet?.Distinct().OrderBy(unicode => unicode).ToArray();
            _atlasSize = atlasSize;
            _atlasPadding = atlasPadding ?? GetDefaultAtlasPadding(renderingStyle);
            _enableMultiAtlas = enableMultiAtlas ?? populationMode == FontAssetPopulationMode.Dynamic;
            ValidateArguments();
        }

        private void ValidateArguments()
        {
            if (string.IsNullOrWhiteSpace(_sourceFontPath))
            {
                throw new ArgumentException("A source font path is required.", nameof(_sourceFontPath));
            }

            if (string.IsNullOrWhiteSpace(_outputAssetPath))
            {
                throw new ArgumentException("An output asset path is required.", nameof(_outputAssetPath));
            }

            if (!_outputAssetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The output path must end with .asset.", nameof(_outputAssetPath));
            }

            if (_samplingPointSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_samplingPointSize));
            }

            if (_atlasSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_atlasSize));
            }

            if (_atlasPadding < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_atlasPadding));
            }

            if (!Enum.IsDefined(typeof(FontRenderingStyle), _renderingStyle))
            {
                throw new ArgumentOutOfRangeException(nameof(_renderingStyle));
            }

            if (!Enum.IsDefined(typeof(FontAssetPopulationMode), _populationMode))
            {
                throw new ArgumentOutOfRangeException(nameof(_populationMode));
            }

            if (_initialCharacterSet is not null && _initialCharacterSet.Any(unicode => unicode is > 0x10FFFF or >= 0xD800 and <= 0xDFFF))
            {
                throw new ArgumentOutOfRangeException(nameof(_initialCharacterSet), "The initial character set contains an invalid Unicode scalar value.");
            }

            if (_populationMode == FontAssetPopulationMode.Static && (_initialCharacterSet is null || _initialCharacterSet.Length == 0))
            {
                throw new InvalidOperationException("Static font generation requires an initial character set.");
            }
        }

        private GlyphRenderMode GetGlyphRenderMode() => _renderingStyle == FontRenderingStyle.Pixel ? GlyphRenderMode.RASTER_HINTED : GlyphRenderMode.SDFAA;

        private AtlasPopulationMode GetAtlasPopulationMode() => _populationMode == FontAssetPopulationMode.Static ? AtlasPopulationMode.Static : AtlasPopulationMode.Dynamic;

        private TMP_FontAsset CreateFontAsset(Font sourceFont)
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                _samplingPointSize,
                _atlasPadding,
                GetGlyphRenderMode(),
                _atlasSize,
                _atlasSize,
                AtlasPopulationMode.Dynamic,
                _enableMultiAtlas);
            if (fontAsset == null)
            {
                throw new InvalidOperationException($"TMP Font Asset creation failed: {_outputAssetPath}");
            }
            fontAsset.name = Path.GetFileNameWithoutExtension(_outputAssetPath);

            AssetDatabase.CreateAsset(fontAsset, _outputAssetPath);

            foreach (var atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture == null)
                {
                    continue;
                }

                atlasTexture.name = $"{fontAsset.name} Atlas";
                AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = $"{fontAsset.name} Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            return fontAsset;
        }

        private void PrepareExistingFontAsset(TMP_FontAsset fontAsset, Font sourceFont)
        {
            var sourceFontGuid = AssetDatabase.AssetPathToGUID(_sourceFontPath);
            var serializedFontAsset = new SerializedObject(fontAsset);
            var sourceFontProperty = serializedFontAsset.FindProperty("m_SourceFontFile");
            var sourceFontGuidProperty = serializedFontAsset.FindProperty("m_SourceFontFileGUID");
            var populationModeProperty = serializedFontAsset.FindProperty("m_AtlasPopulationMode");

            if (sourceFontProperty is null || sourceFontGuidProperty is null || populationModeProperty is null)
            {
                throw new InvalidOperationException("The installed TextMesh Pro version is not compatible with font asset regeneration.");
            }

            if (!string.IsNullOrEmpty(sourceFontGuidProperty.stringValue) && sourceFontGuidProperty.stringValue != sourceFontGuid)
            {
                throw new InvalidOperationException($"The existing font asset was generated from a different source font. Delete it once and generate it again: {_outputAssetPath}");
            }

            if (!Mathf.Approximately(fontAsset.faceInfo.pointSize, _samplingPointSize) ||
                fontAsset.atlasRenderMode != GetGlyphRenderMode() ||
                fontAsset.atlasPadding != _atlasPadding ||
                fontAsset.atlasWidth != _atlasSize ||
                fontAsset.atlasHeight != _atlasSize)
            {
                throw new InvalidOperationException($"The existing font asset uses different generation settings. Delete it once and generate it again: {_outputAssetPath}");
            }

            sourceFontProperty.objectReferenceValue = sourceFont;
            sourceFontGuidProperty.stringValue = sourceFontGuid;
            populationModeProperty.enumValueIndex = (int)AtlasPopulationMode.Dynamic;

            serializedFontAsset.ApplyModifiedPropertiesWithoutUndo();

            fontAsset.ClearFontAssetData(true);
        }

        private void ApplyFinalPopulationMode(TMP_FontAsset fontAsset, Font sourceFont)
        {
            var serializedFontAsset = new SerializedObject(fontAsset);
            var sourceFontProperty = serializedFontAsset.FindProperty("m_SourceFontFile");
            var populationModeProperty = serializedFontAsset.FindProperty("m_AtlasPopulationMode");

            if (sourceFontProperty is null || populationModeProperty is null)
            {
                throw new InvalidOperationException("The installed TextMesh Pro version is not compatible with font asset generation.");
            }

            populationModeProperty.enumValueIndex = (int)GetAtlasPopulationMode();
            sourceFontProperty.objectReferenceValue = _populationMode == FontAssetPopulationMode.Dynamic ? sourceFont : null;
            serializedFontAsset.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AddInitialCharacters(TMP_FontAsset fontAsset)
        {
            if (_initialCharacterSet is null || _initialCharacterSet.Length == 0)
            {
                return;
            }

            var addedAllCharacters = fontAsset.TryAddCharacters(_initialCharacterSet, out var missingUnicodes);
            if (!addedAllCharacters || missingUnicodes is { Length: > 0 })
            {
                var message = new StringBuilder();
                message.Append(Path.GetFileNameWithoutExtension(_outputAssetPath));
                message.Append(" could not add these characters. The source font may not contain them, or the atlas may not have enough space:");

                foreach (var unicode in missingUnicodes.Distinct())
                {
                    message.AppendLine();
                    message.Append("U+");
                    message.Append(unicode.ToString("X4"));
                    message.Append(" ");

                    if (unicode <= 0x10FFFF)
                    {
                        message.Append(char.ConvertFromUtf32((int)unicode));
                    }
                }

                Debug.LogWarning(message.ToString(), fontAsset);
            }
        }

        private void ConfigureAtlasTextures(TMP_FontAsset fontAsset)
        {
            var filterMode = _renderingStyle == FontRenderingStyle.Pixel ? FilterMode.Point : FilterMode.Bilinear;

            foreach (var atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture == null)
                {
                    continue;
                }

                atlasTexture.filterMode = filterMode;
                atlasTexture.wrapMode = TextureWrapMode.Clamp;

                EditorUtility.SetDirty(atlasTexture);
            }

            if (fontAsset.material == null)
            {
                return;
            }

            var mainTexture = fontAsset.material.mainTexture;
            if (mainTexture != null)
            {
                mainTexture.filterMode = filterMode;
                mainTexture.wrapMode = TextureWrapMode.Clamp;
            }

            EditorUtility.SetDirty(fontAsset.material);
        }

        public TMP_FontAsset Generate()
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(_sourceFontPath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException($"Source font was not found: {_sourceFontPath}");
            }

            if (!sourceFont.dynamic)
            {
                throw new InvalidOperationException($"The source font must use dynamic font rendering: {_sourceFontPath}");
            }

            var outputFolder = Path.GetDirectoryName(_outputAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(outputFolder) || !AssetDatabase.IsValidFolder(outputFolder))
            {
                throw new InvalidOperationException($"Font output folder was not found: {outputFolder}");
            }

            var existingAsset = AssetDatabase.LoadMainAssetAtPath(_outputAssetPath);
            if (existingAsset != null && existingAsset is not TMP_FontAsset)
            {
                throw new InvalidOperationException($"The output path contains a different asset type: {_outputAssetPath}");
            }

            var fontAsset = existingAsset as TMP_FontAsset;
            if (fontAsset == null)
            {
                fontAsset = CreateFontAsset(sourceFont);
            }
            else
            {
                PrepareExistingFontAsset(fontAsset, sourceFont);
            }
            fontAsset.isMultiAtlasTexturesEnabled = _enableMultiAtlas;

            AddInitialCharacters(fontAsset);
            ConfigureAtlasTextures(fontAsset);
            ApplyFinalPopulationMode(fontAsset, sourceFont);

            if (fontAsset.atlasPopulationMode != GetAtlasPopulationMode())
            {
                throw new InvalidOperationException($"The generated font asset did not retain the requested population mode: {_outputAssetPath}");
            }

            if (_populationMode == FontAssetPopulationMode.Dynamic && fontAsset.sourceFontFile != sourceFont)
            {
                throw new InvalidOperationException($"The generated dynamic font asset did not retain its source font: {_outputAssetPath}");
            }

            if (_populationMode == FontAssetPopulationMode.Static && fontAsset.sourceFontFile != null)
            {
                throw new InvalidOperationException($"The generated static font asset retained a runtime source font: {_outputAssetPath}");
            }

            EditorUtility.SetDirty(fontAsset);

            Debug.Log($"{fontAsset.name}: generated {fontAsset.characterTable.Count} characters in {fontAsset.atlasTextureCount} atlas texture(s).", fontAsset);

            return fontAsset;
        }
    }
}
