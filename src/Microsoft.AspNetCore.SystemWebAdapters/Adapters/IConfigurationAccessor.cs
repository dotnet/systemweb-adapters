namespace Microsoft.AspNetCore.SystemWebAdapters.Adapters;

internal interface IConfigurationAccessor
{
    string? GetSetting(string key);

    string? GetConnectionString(string name);
}
