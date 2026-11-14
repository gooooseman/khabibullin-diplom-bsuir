using CommunityToolkit.Mvvm.ComponentModel;
using FinancialAssistant.UI.Localization;
using System;
using System.ComponentModel;
using L = FinancialAssistant.UI.Localization.Localization;

namespace FinancialAssistant.UI.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                SetProperty(ref isBusy, value);
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        // Raw override — set this if you want a fixed string (not translated)
        private string _titleOverride = string.Empty;

        // Localization key — set this to get auto-updating translated title
        private string _titleKey = string.Empty;

        public string Title
        {
            get => string.IsNullOrEmpty(_titleKey)
                ? _titleOverride
                : L.T(_titleKey);
            set
            {
                // Treat as a raw override (no key)
                _titleKey = string.Empty;
                if (SetProperty(ref _titleOverride, value))
                    OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Set a localization key so Title auto-updates when the language changes.
        /// </summary>
        protected void SetTitleKey(string key)
        {
            _titleKey = key;
            _titleOverride = string.Empty;
            OnPropertyChanged(nameof(Title));

            // Subscribe once to language changes
            LocalizationSource.Instance.PropertyChanged -= OnLocalizationChanged;
            LocalizationSource.Instance.PropertyChanged += OnLocalizationChanged;
        }

        private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_titleKey))
                OnPropertyChanged(nameof(Title));
        }

        public bool IsNotBusy => !isBusy;

        protected void LogError(string message, Exception? ex = null)
        {
            System.Diagnostics.Debug.WriteLine($"[ViewModel Error] {message}: {ex}");
        }
    }
}
