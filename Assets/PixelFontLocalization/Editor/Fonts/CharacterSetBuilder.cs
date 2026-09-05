using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization;
using UnityEngine.Localization;

namespace PixelFontLocalization.Editor.Fonts
{
    public static class CharacterSetBuilder
    {
        private static void AddText(ISet<uint> characterSet, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];

                if (char.IsHighSurrogate(character) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                {
                    characterSet.Add((uint)char.ConvertToUtf32(character, text[++index]));
                    continue;
                }

                if (!char.IsSurrogate(character) && !char.IsControl(character))
                {
                    characterSet.Add(character);
                }
            }
        }

        private static bool IsValidUnicode(uint unicode) => unicode is <= 0x10FFFF and (< 0xD800 or > 0xDFFF);

        public static uint[] Combine(params IEnumerable<uint>[] characterSets)
        {
            if (characterSets is null)
            {
                throw new ArgumentNullException(nameof(characterSets));
            }

            var result = new SortedSet<uint>();
            foreach (var characterSet in characterSets)
            {
                if (characterSet is null)
                {
                    continue;
                }

                foreach (var unicode in characterSet)
                {
                    if (IsValidUnicode(unicode))
                    {
                        result.Add(unicode);
                    }
                }
            }
            return result.ToArray();
        }

        public static uint[] FromTexts(IEnumerable<string> texts)
        {
            if (texts is null)
            {
                throw new ArgumentNullException(nameof(texts));
            }

            var characterSet = new SortedSet<uint>();
            foreach (var text in texts)
            {
                AddText(characterSet, text);
            }
            return characterSet.ToArray();
        }

        public static uint[] FromText(string text) => FromTexts(new[] { text });

        public static uint[] BasicAscii() => Enumerable.Range(32, 95).Select(value => (uint)value).ToArray();

        public static uint[] TextMeshProDefaults() => Combine(BasicAscii(), new uint[] { 0x00A0, 0x200B, 0x2026, 0x25A1 });
    }

    public static class LocalizationCharacterSetBuilder
    {
        private static void ValidateArguments(IReadOnlyCollection<string> tableCollectionNames, IReadOnlyCollection<string> localeCodes)
        {
            if (tableCollectionNames is null || tableCollectionNames.Count == 0)
            {
                throw new ArgumentException("At least one String Table Collection is required.", nameof(tableCollectionNames));
            }

            if (localeCodes is null || localeCodes.Count == 0)
            {
                throw new ArgumentException("At least one Locale code is required.", nameof(localeCodes));
            }

            if (tableCollectionNames.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("String Table Collection names cannot be empty.", nameof(tableCollectionNames));
            }

            if (localeCodes.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Locale codes cannot be empty.", nameof(localeCodes));
            }
        }

        public static uint[] Build(IReadOnlyCollection<string> tableCollectionNames, IReadOnlyCollection<string> localeCodes)
        {
            ValidateArguments(tableCollectionNames, localeCodes);

            var localeIdentifiers = localeCodes.Select(code => new LocaleIdentifier(code)).ToArray();

            var characterSets = new List<IEnumerable<uint>>();

            foreach (var collectionName in tableCollectionNames)
            {
                var collection = LocalizationEditorSettings.GetStringTableCollection(collectionName);
                if (collection == null)
                {
                    throw new InvalidOperationException($"String Table Collection was not found: {collectionName}");
                }

                var characters = collection.GenerateCharacterSet(localeIdentifiers);
                characterSets.Add(CharacterSetBuilder.FromText(characters));
            }

            return CharacterSetBuilder.Combine(characterSets.ToArray());
        }
    }
}
