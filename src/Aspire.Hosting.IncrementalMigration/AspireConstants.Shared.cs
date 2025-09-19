using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static partial class AspireConstants
{
    public const string DefaultIncrementalServiceName = "Default";

    public const string ProxyKey = "Proxy";
    public const string ProxyKeyIsEnabled = ProxyKey + Separator + "UseForwardedHeaders";
    public const string RemoteKey = "Remote";
    public const string RemoteApiKey = RemoteKey + Separator + "ApiKey";
    public const string RemoteUrl = RemoteKey + Separator + "RemoteAppUrl";
    public const string RemoteSessionKey = RemoteKey + Separator + "Session";
    public const string RemoteAuthKey = RemoteKey + Separator + "Authentication";
    public const string RemoteAuthIsDefaultScheme = RemoteAuthKey + Separator + "IsDefaultScheme";
    public const string IsEnabled = Separator + "IsEnabled";

    public static string GetConfigSection(string name) => $"IncrementalMigration{Separator}{name}";

    public static string GetKey(string name, string key) => $"{GetConfigSection(name)}{Separator}{key}";
}
