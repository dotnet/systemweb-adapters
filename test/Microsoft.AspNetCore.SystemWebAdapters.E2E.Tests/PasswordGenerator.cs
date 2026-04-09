using System.Security.Cryptography;

namespace Microsoft.AspNetCore.SystemWebAdapters.E2E.Tests;

internal static class PasswordGenerator
{
    // Passwords must have at least 8 characters with lower, upper, numbers, and symbols
    public static string CreatePassword()
    {
        var lower = "abcdefghijklmnopqrstuvwxyz";
        var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var numbers = "0123456789";
        var symbols = "!@#$%^&*()";

        Span<char> str = stackalloc char[8];

        RandomNumberGenerator.GetItems(lower, str.Slice(0, 2));
        RandomNumberGenerator.GetItems(upper, str.Slice(2, 2));
        RandomNumberGenerator.GetItems(numbers, str.Slice(4, 2));
        RandomNumberGenerator.GetItems(symbols, str.Slice(6, 2));

        RandomNumberGenerator.Shuffle(str);

        return new string(str);
    }
}
