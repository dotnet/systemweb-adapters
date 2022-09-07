// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Util;

/// <summary>
/// Provides utility methods for string operations.
/// </summary>
/// <remarks>
/// Portions of this code are based on code originally Copyright (c) 1999 Microsoft Corporation
/// System.Web.Util.StringUtil at https://github.com/microsoft/referencesource/blob/master/System.Web/Util/StringUtil.cs
/// These files are released under an MIT licence according to https://github.com/microsoft/referencesource#license
/// </remarks>
internal static class StringUtil
{
    /*
     * Determines if the first string starts with the second string, ignoring case.
     * Fast, non-culture aware.  
     */
    internal static bool StringStartsWithIgnoreCase(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return false;
        if (s2.Length > s1.Length) return false;
        return s1.StartsWith(s2, StringComparison.OrdinalIgnoreCase);
    }
}
