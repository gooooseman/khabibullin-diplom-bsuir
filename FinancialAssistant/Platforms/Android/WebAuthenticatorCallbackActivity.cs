using Android.App;
using Android.Content;
using Android.Content.PM;

namespace FinancialAssistant
{
    [Activity(
        Name = "com.companyname.financialassistant.WebAuthenticatorCallbackActivity",
        NoHistory = true,
        LaunchMode = LaunchMode.SingleTop,
        Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.companyname.financialassistant",
        DataHost = "oauth2redirect")]
    public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}
