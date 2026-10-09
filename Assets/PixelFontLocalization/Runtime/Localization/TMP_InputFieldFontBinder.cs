using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class TMP_InputFieldFontBinder : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField target;

        [SerializeField]
        private LocalizedTmpFont localizedFont = new();
        
        private void Reset() => target = GetComponent<TMP_InputField>();

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<TMP_InputField>();
            }
        }

        private void OnEnable() => localizedFont.AssetChanged += OnFontChanged;

        private void OnDisable() => localizedFont.AssetChanged -= OnFontChanged;

        private void OnFontChanged(TMP_FontAsset fontAsset)
        {
            target.fontAsset = fontAsset;
            target.textComponent.ForceMeshUpdate();
            target.ForceLabelUpdate();
        }
    }
}
