// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal sealed class HttpHandlerRouteMetadata
{
    private readonly Regex _regex; 

    public HttpHandlerRouteMetadata(string filePath)
    {
        Path = filePath;
        _regex = Create(filePath);
    }

    public string Path { get; }

    public bool IsMatch(string path) => _regex.IsMatch(path);

    public (string FilePath, string PathInfo) GetInfo(string path)
    {
        if (_regex.Match(path) is { Success: true, Groups.Count: > 2 } match)
        {
            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        return (path, string.Empty);
    }

    // Converts a path to a regex string
    //   - Can be relative 'some/path.txt' or absolte '/some/path.txt'
    //   - May have a path of interest after file, i.e. 'some/path.txt' should match '/web/some/path.txt/sub'
    private static Regex Create(string path)
    {
        var sb = new StringBuilder(Regex.Escape(path));

        sb.Replace(@"\*", ".*");

        if (path.StartsWith('/'))
        {
            sb.Insert(0, "^(");
        }
        else
        {
            sb.Insert(0, @"(.*\/");
        }

        sb.Append(@")(\/.*|$)");

        var pattern = sb.ToString();

        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);
    }
}

