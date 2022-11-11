using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal;

internal abstract class NoGetByIntNameValueCollection : HttpValueCollection
{
    private protected const string IndexErrorMessage = "ASP.NET Core doesn't support accessing items by index.";

    public sealed override string? Get(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

    public sealed override string? GetKey(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

    public sealed override string[]? GetValues(int index) => throw new PlatformNotSupportedException(IndexErrorMessage);

    public sealed override KeysCollection Keys => throw new PlatformNotSupportedException(IndexErrorMessage);
}
