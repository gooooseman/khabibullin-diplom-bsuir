using System;
using Microsoft.Maui.Controls;
using FinancialAssistant.UI.Localization;

namespace FinancialAssistant.UI.Markup
{
    /// <summary>
    /// Markup extension that returns a live Binding to LocalizationSource.Instance[key].
    /// This means the UI automatically updates when the language is changed at runtime
    /// without requiring an app restart.
    ///
    /// Usage in XAML:  Text="{loc:Translate Text=MyKey}"
    /// </summary>
    [ContentProperty(nameof(Text))]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        public string Text { get; set; } = string.Empty;

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{Text}]",
                Source = LocalizationSource.Instance
            };
        }

        // Explicit interface implementation for non-generic IMarkupExtension
        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
            => ProvideValue(serviceProvider);
    }
}
