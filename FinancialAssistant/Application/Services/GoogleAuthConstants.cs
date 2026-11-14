namespace FinancialAssistant.Application.Services;

public static class GoogleAuthConstants
{
    // Android Client ID — supports custom scheme redirect natively
    public const string AndroidClientId = "565162610680-7t4e3bkdo2vo71ds4dpgqjs82ak842ki.apps.googleusercontent.com";

    // Web Client ID — used for token validation
    public const string WebClientId = "565162610680-t8dbpbhquavrlhsded756mta4tium1ld.apps.googleusercontent.com";

    // Custom scheme redirect — registered automatically for Android Client ID
    public const string RedirectUri = "com.companyname.financialassistant:/oauth2redirect";

    // Use Android Client ID for the OAuth flow
    public const string ClientId = AndroidClientId;
}
