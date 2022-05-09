namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class SharedAuthCookieOptions
{
    /// <summary>
    /// Gets or sets the unique name of this application within the data protection system.
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the name to use for the cookie that will be shared between apps.
    /// </summary>
    public string CookieName { get; set; }

    /// <summary>
    /// Options configuring how authentication cookies are shared bewtween apps.
    /// </summary>
    /// <param name="applicationName">The unique name of this application within the data protection system.</param>
    /// <param name="cookieName">The name to use for the cookie that will be shared between apps.</param>
    public SharedAuthCookieOptions(string applicationName, string cookieName = ".AspNet.SharedCookie")
    {
        ApplicationName = applicationName;
        CookieName = cookieName;
    }

}
