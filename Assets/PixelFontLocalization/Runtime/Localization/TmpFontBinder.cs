using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PixelFontLocalization.Runtime.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpFontBinder : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text target;

        [SerializeField]
        private LocalizedTmpFont localizedFont = new();

        [SerializeField]
        private Locale localeOverride;

        public Locale LocaleOverride => localeOverride;

        private AsyncOperationHandle<TMP_FontAsset> loadingOperation;

        private void Reset() => target = GetComponent<TMP_Text>();

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<TMP_Text>();
            }

            if (localeOverride != null)
            {
                localizedFont.LocaleOverride = localeOverride;
                loadingOperation = Addressables.ResourceManager.Acquire(localizedFont.LoadAssetAsync());
                loadingOperation.Completed += OnGetFontCompleted;
            }
        }

        private void OnDestroy()
        {
            if (loadingOperation.IsValid())
            {
                loadingOperation.Completed -= OnGetFontCompleted;
                loadingOperation.Release();
                loadingOperation = default;
            }
        }

        private void OnGetFontCompleted(AsyncOperationHandle<TMP_FontAsset> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                OnFontChanged(operation.Result);
            }
        }

        private void OnEnable()
        {
            if (localeOverride == null)
            {
                localizedFont.AssetChanged += OnFontChanged;
            }
        }

        private void OnDisable()
        {
            if (localeOverride == null)
            {
                localizedFont.AssetChanged -= OnFontChanged;
            }
        }

        private void OnFontChanged(TMP_FontAsset fontAsset) => target.font = fontAsset;
    }
}
