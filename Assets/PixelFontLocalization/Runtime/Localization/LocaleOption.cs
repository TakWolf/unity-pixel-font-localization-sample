using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(Button))]
    public sealed class LocaleOption : MonoBehaviour
    {
        [SerializeField] private Button button;

        [SerializeField] private TmpStringBinder stringBinder;

        public event Action<Locale> OptionSelected;

        private void Reset()
        {
            button = GetComponent<Button>();
            stringBinder = GetComponentInChildren<TmpStringBinder>(true);
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            if (stringBinder == null)
            {
                stringBinder = GetComponentInChildren<TmpStringBinder>(true);
            }
        }

        public Locale Locale => stringBinder.LocaleOverride;

        public bool IsSelected
        {
            get => !button.interactable;
            set => button.interactable = !value;
        }

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnButtonClick() => OptionSelected?.Invoke(Locale);
    }
}
