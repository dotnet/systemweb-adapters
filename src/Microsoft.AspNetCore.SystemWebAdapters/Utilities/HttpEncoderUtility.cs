// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.AspNetCore.SystemWebAdapters.Utilities;

internal static class HttpEncoderUtility
{
    public static char IntToHex(int n)
    {
        Debug.Assert(n < 0x10);

        return n <= 9 ? (char)(n + '0') : (char)(n - 10 + 'a');
    }

    // Set of safe chars, from RFC 1738.4 minus '+'
    public static bool IsUrlSafeChar(char ch)
    {
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
        {
            return true;
        }

        switch (ch)
        {
            case '-':
            case '_':
            case '.':
            case '!':
            case '*':
            case '(':
            case ')':
                return true;
        }

        return false;
    }
}
