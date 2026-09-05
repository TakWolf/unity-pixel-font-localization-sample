using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace PixelFontLocalization.Runtime.Localization
{
    public sealed class LanguageSettingsPanel : MonoBehaviour
    {
        private GameObject dialog;
        private LocaleOption[] options;

        private void Awake()
        {
            if (dialog == null)
            {
                dialog = transform.Find("Dialog").gameObject;
            }
            if (options == null)
            {
                options = dialog.transform.Find("LocaleOptionsGrid").gameObject.GetComponentsInChildren<LocaleOption>();
            }

            dialog.SetActive(false);
        }

        public void ShowDialog()
        {
            if (dialog.activeSelf)
            {
                return;
            }

            foreach (var option in options)
            {
                option.OptionSelected += UpdateOptionsSelected;
            }

            UpdateOptionsSelected(LocalizationSettings.SelectedLocale);
            dialog.SetActive(true);
        }

        public void HideDialog()
        {
            if (!dialog.activeSelf)
            {
                return;
            }

            foreach (var option in options)
            {
                option.OptionSelected -= UpdateOptionsSelected;
            }

            dialog.SetActive(false);
        }

        private void UpdateOptionsSelected(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
            foreach (var option in options)
            {
                option.IsSelected = option.Locale == locale;
            }
        }
    }
}
