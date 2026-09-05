using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpStringBinder : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text target;

        [SerializeField]
        private LocalizedString localizedString = new();

        [SerializeField]
        private Locale localeOverride;

        public Locale LocaleOverride => localeOverride;

        private AsyncOperationHandle<string> loadingOperation;

        private void Reset() => target = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }

            if (localeOverride != null)
            {
                localizedString.LocaleOverride = localeOverride;
                loadingOperation = Addressables.ResourceManager.Acquire(localizedString.GetLocalizedStringAsync());
                loadingOperation.Completed += OnGetStringCompleted;
            }
        }

        private void OnDestroy()
        {
            if (loadingOperation.IsValid())
            {
                loadingOperation.Completed -= OnGetStringCompleted;
                loadingOperation.Release();
                loadingOperation = default;
            }
        }

        private void OnGetStringCompleted(AsyncOperationHandle<string> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                OnStringChanged(operation.Result);
            }
        }

        private void OnEnable()
        {
            if (localeOverride == null)
            {
                localizedString.StringChanged += OnStringChanged;
            }
        }

        private void OnDisable()
        {
            if (localeOverride == null)
            {
                localizedString.StringChanged -= OnStringChanged;
            }
        }

        private void OnStringChanged(string text) => target.text = text;
    }
}
