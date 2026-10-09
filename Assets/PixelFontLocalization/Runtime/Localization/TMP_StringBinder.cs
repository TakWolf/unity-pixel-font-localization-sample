using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMP_StringBinder : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text target;

        [SerializeField]
        private LocalizedString localizedString = new();

        private void Reset() => target = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }
        }
        
        private void OnEnable() => localizedString.StringChanged += OnStringChanged;

        private void OnDisable() => localizedString.StringChanged -= OnStringChanged;

        private void OnStringChanged(string text) => target.text = text;
    }
}
