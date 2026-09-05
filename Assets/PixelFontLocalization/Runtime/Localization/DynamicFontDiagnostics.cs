using TMPro;
using UnityEngine;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class DynamicFontDiagnostics : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;

        [SerializeField]
        private TMP_Text output;

        private TMP_FontAsset _lastFontAsset;
        private int _lastCharacterCount = -1;
        private int _lastAtlasCount = -1;

        private void Reset() => inputField = GetComponent<TMP_InputField>();

        private void Awake()
        {
            if (inputField == null)
            {
                inputField = GetComponent<TMP_InputField>();
            }
        }

        private void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnValueChanged);
            RefreshIfChanged();
        }

        private void OnDisable() => inputField.onValueChanged.RemoveListener(OnValueChanged);

        private void LateUpdate() => RefreshIfChanged();

        private void OnValueChanged(string value)
        {
            inputField.textComponent.ForceMeshUpdate();
            RefreshIfChanged();
        }

        private void RefreshIfChanged()
        {
            if (output == null)
            {
                return;
            }

            var fontAsset = inputField.fontAsset;
            if (fontAsset == null)
            {
                if (_lastFontAsset != null)
                {
                    _lastFontAsset = null;
                    _lastCharacterCount = -1;
                    _lastAtlasCount = -1;
                }

                output.text = "Font: Loading...";
                return;
            }

            var characterCount = fontAsset.characterTable.Count;
            var atlasCount = fontAsset.atlasTextureCount;

            if (_lastFontAsset == fontAsset && _lastCharacterCount == characterCount && _lastAtlasCount == atlasCount)
            {
                return;
            }

            _lastFontAsset = fontAsset;
            _lastCharacterCount = characterCount;
            _lastAtlasCount = atlasCount;

            output.text =
                $"Font: {fontAsset.name}\n" +
                $"Characters: {characterCount}\n" +
                $"Atlases: {atlasCount}";
        }
    }
}
