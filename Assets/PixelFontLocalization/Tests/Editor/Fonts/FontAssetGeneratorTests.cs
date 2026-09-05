using System;
using NUnit.Framework;
using PixelFontLocalization.Editor.Fonts;

namespace PixelFontLocalization.Tests.Editor.Fonts
{
    public sealed class FontAssetGeneratorTests
    {
        private const string SourceFontPath = "Assets/PixelFontLocalization/Fonts/fusion-pixel-12px/fusion-pixel-12px-proportional-latin.otf";
        private const string OutputAssetPath = "Assets/PixelFontLocalization/Fonts/FontAssets/Test.asset";

        private static FontAssetGenerator Create(
            string sourceFontPath = SourceFontPath,
            string outputAssetPath = OutputAssetPath,
            int samplingPointSize = 12,
            FontRenderingStyle renderingStyle = FontRenderingStyle.Pixel,
            FontAssetPopulationMode populationMode = FontAssetPopulationMode.Dynamic,
            uint[] initialCharacterSet = null,
            int atlasSize = 1024,
            int? atlasPadding = null)
        {
            return new FontAssetGenerator(
                sourceFontPath,
                outputAssetPath,
                samplingPointSize,
                renderingStyle,
                populationMode,
                initialCharacterSet,
                atlasSize,
                atlasPadding);
        }

        [Test]
        public void Constructor_RejectsEmptySourcePath()
        {
            Assert.Throws<ArgumentException>(() => Create(sourceFontPath: string.Empty));
        }

        [Test]
        public void Constructor_RejectsOutputPathWithoutAssetExtension()
        {
            Assert.Throws<ArgumentException>(() => Create(outputAssetPath: "Assets/Test.font"));
        }

        [Test]
        public void Constructor_RejectsNonPositiveSamplingPointSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(samplingPointSize: 0));
        }

        [Test]
        public void Constructor_RejectsNonPositiveAtlasSize()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(atlasSize: 0));
        }

        [Test]
        public void Constructor_RejectsNegativeAtlasPadding()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(atlasPadding: -1));
        }

        [Test]
        public void Constructor_RejectsUnknownRenderingStyle()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(renderingStyle: (FontRenderingStyle)99));
        }

        [Test]
        public void Constructor_RejectsUnknownPopulationMode()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(populationMode: (FontAssetPopulationMode)99));
        }

        [TestCase(0xD800u)]
        [TestCase(0xDFFFu)]
        [TestCase(0x110000u)]
        public void Constructor_RejectsInvalidUnicodeScalar(uint unicode)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(initialCharacterSet: new[] { unicode }));
        }

        [Test]
        public void Constructor_RejectsStaticModeWithoutInitialCharacters()
        {
            Assert.Throws<InvalidOperationException>(() => Create(populationMode: FontAssetPopulationMode.Static, initialCharacterSet: null));
        }

        [Test]
        public void Constructor_AcceptsDynamicModeWithoutInitialCharacters()
        {
            Assert.DoesNotThrow(() => Create(initialCharacterSet: null));
        }
    }
}
