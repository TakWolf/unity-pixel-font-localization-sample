using System;
using System.Linq;
using NUnit.Framework;
using PixelFontLocalization.Editor.Fonts;

namespace PixelFontLocalization.Tests.Editor.Fonts
{
    public sealed class CharacterSetBuilderTests
    {
        [Test]
        public void FromText_ReturnsSortedDistinctUnicodeScalars()
        {
            var result = CharacterSetBuilder.FromText("B😀A😀B");

            CollectionAssert.AreEqual(new uint[] { 0x41, 0x42, 0x1F600 }, result);
        }

        [Test]
        public void FromText_FiltersControlAndUnpairedSurrogateCharacters()
        {
            var text = string.Concat("A\n", '\uD800', "B", '\uDC00');

            var result = CharacterSetBuilder.FromText(text);

            CollectionAssert.AreEqual(new uint[] { 0x41, 0x42 }, result);
        }

        [Test]
        public void FromTexts_IgnoresNullAndEmptyValues()
        {
            var result = CharacterSetBuilder.FromTexts(new[] { null, string.Empty, "BA", "C" });

            CollectionAssert.AreEqual(new uint[] { 0x41, 0x42, 0x43 }, result);
        }

        [Test]
        public void FromTexts_ThrowsWhenCollectionIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => CharacterSetBuilder.FromTexts(null));
        }

        [Test]
        public void Combine_FiltersInvalidUnicodeAndIgnoresNullCollections()
        {
            var result = CharacterSetBuilder.Combine(
                new uint[] { 0x42, 0x41, 0x42, 0x10FFFF, 0x110000, 0xD800 },
                null,
                new uint[] { 0x1F600 });

            CollectionAssert.AreEqual(new uint[] { 0x41, 0x42, 0x1F600, 0x10FFFF }, result);
        }

        [Test]
        public void Combine_ThrowsWhenCollectionsArrayIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => CharacterSetBuilder.Combine(null));
        }

        [Test]
        public void BasicAscii_ReturnsPrintableAsciiRange()
        {
            var result = CharacterSetBuilder.BasicAscii();

            Assert.That(result, Has.Length.EqualTo(95));
            Assert.That(result.First(), Is.EqualTo(0x20));
            Assert.That(result.Last(), Is.EqualTo(0x7E));
        }

        [Test]
        public void TextMeshProDefaults_ContainsRequiredCharacters()
        {
            var result = CharacterSetBuilder.TextMeshProDefaults();

            CollectionAssert.IsSubsetOf(new uint[] { 0x00A0, 0x200B, 0x2026, 0x25A1 }, result);
        }
    }
}
