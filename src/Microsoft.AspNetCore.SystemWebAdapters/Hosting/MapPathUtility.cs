using System.IO;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace System.Web.Hosting;

internal sealed class MapPathUtility : IMapPathUtility
{
    private readonly IOptions<HostingEnvironmentOptions> _options;
    private readonly VirtualPathUtilityImpl _pathUtility;

    public MapPathUtility(IOptions<HostingEnvironmentOptions> options, VirtualPathUtilityImpl pathUtility)
    {
        _options = options;
        _pathUtility = pathUtility;
    }

    public string MapPath(string requestPath, string? path)
    {
        var appPath = string.IsNullOrEmpty(path)
            ? VirtualPathUtilityImpl.GetDirectory(requestPath)
            : _pathUtility.Combine(VirtualPathUtilityImpl.GetDirectory(requestPath) ?? "/", path);

        var rootPath = _options.Value.AppDomainAppPath;

        if (string.IsNullOrEmpty(appPath))
        {
            return rootPath;
        }

        return Path.Combine(
            rootPath,
            appPath[1..]
            .Replace('/', Path.DirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar);
    }
}
