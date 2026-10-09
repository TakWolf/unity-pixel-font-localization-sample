using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMP_FontBinder : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text target;

        [SerializeField]
        private LocalizedTmpFont localizedFont = new();
        
        private void Reset() => target = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }
        }

        private void OnEnable() => localizedFont.AssetChanged += OnFontChanged;

        private void OnDisable() => localizedFont.AssetChanged -= OnFontChanged;

        private void OnFontChanged(TMP_FontAsset fontAsset) => target.font = fontAsset;
    }
}
