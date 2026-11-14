using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace FinancialAssistant.UI.Localization
{
    /// <summary>
    /// Observable localization source. All {loc:Translate} bindings point here,
    /// so raising PropertyChanged("Item[]") refreshes every translated string at once.
    /// </summary>
    public sealed class LocalizationSource : INotifyPropertyChanged
    {
        public static readonly LocalizationSource Instance = new();

        private readonly ResourceManager _rm =
            new ResourceManager("FinancialAssistant.Resources.Strings.AppResources",
                                typeof(LocalizationSource).Assembly);

        private CultureInfo _culture = GetStartupCulture();

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (Equals(_culture, value)) return;
                _culture = value;
                // Notify all indexed bindings so every {loc:Translate} refreshes
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        /// <summary>Indexer used by TranslateExtension binding.</summary>
        public string this[string key]
        {
            get
            {
                try { return _rm.GetString(key, _culture) ?? key; }
                catch { return key; }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private static CultureInfo GetStartupCulture()
        {
            try
            {
                var saved = Microsoft.Maui.Storage.Preferences.Get("AppLanguage", "");
                if (!string.IsNullOrEmpty(saved))
                    return new CultureInfo(saved);
            }
            catch { }
            var sys = CultureInfo.CurrentUICulture;
            return sys.TwoLetterISOLanguageName == "ru" ? new CultureInfo("ru") : new CultureInfo("en");
        }
    }

    /// <summary>
    /// Compatibility shim — keeps old Localization.T() calls working.
    /// </summary>
    public static class Localization
    {
        public static CultureInfo Culture => LocalizationSource.Instance.Culture;

        public static void SetCulture(CultureInfo culture)
            => LocalizationSource.Instance.Culture = culture;

        public static string T(string key) => LocalizationSource.Instance[key];
    }
}
