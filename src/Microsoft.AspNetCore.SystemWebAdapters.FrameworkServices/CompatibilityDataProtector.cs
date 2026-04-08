// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters.Hosting;

namespace Microsoft.AspNetCore.DataProtection.SystemWeb;

[EditorBrowsable(EditorBrowsableState.Never)]
public class CompatibilityDataProtector : DataProtector
{
    [ThreadStatic]
    private static bool _suppressPrimaryPurpose;

    private readonly Lazy<IDataProtector> _lazyProtector;
    private readonly Lazy<IDataProtector> _lazyProtectorSuppressedPrimaryPurpose;

    public CompatibilityDataProtector(string applicationName, string primaryPurpose, string[] specificPurposes)
        : base("application-name", "primary-purpose", null) // we feed dummy values to the base ctor
    {
        // We don't want to evaluate the IDataProtectionProvider factory quite yet,
        // as we'd rather defer failures to the call to Protect so that we can bubble
        // up a good error message to the developer.

        _lazyProtector = new Lazy<IDataProtector>(() => GetDataProtectionProvider().CreateProtector(primaryPurpose, specificPurposes));

        // System.Web always provides "User.MachineKey.Protect" as the primary purpose for calls
        // to MachineKey.Protect. Only in this case should we allow suppressing the primary
        // purpose, as then we can easily map calls to MachineKey.Protect(userData, purposes)
        // into calls to provider.GetProtector(purposes).Protect(userData).
        if (primaryPurpose == "User.MachineKey.Protect")
        {
            _lazyProtectorSuppressedPrimaryPurpose = new Lazy<IDataProtector>(() => GetDataProtectionProvider().CreateProtector(specificPurposes));
        }
        else
        {
            _lazyProtectorSuppressedPrimaryPurpose = _lazyProtector;
        }
    }

    // We take care of flowing purposes ourselves.
    protected override bool PrependHashedPurposeToPlaintext { get; }

    // Retrieves the appropriate protector (potentially with a suppressed primary purpose) for this operation.
    private IDataProtector Protector => ((_suppressPrimaryPurpose) ? _lazyProtectorSuppressedPrimaryPurpose : _lazyProtector).Value;

    protected virtual IDataProtectionProvider GetDataProtectionProvider()
        => HttpApplicationHost.Current.Services.GetDataProtectionProvider();

    public override bool IsReprotectRequired(byte[] encryptedData)
    {
        // Nobody ever calls this.
        return false;
    }

    protected override byte[] ProviderProtect(byte[] userData)
    {
        try
        {
            return Protector.Protect(userData);
        }
        catch (Exception ex)
        {
            // System.Web special-cases ConfigurationException errors and allows them to bubble
            // up to the developer without being homogenized. Since a call to Protect should
            // never fail, any exceptions here really do imply a misconfiguration.

#pragma warning disable CS0618 // Type or member is obsolete
            throw new ConfigurationException("DataProtection failed to protect", ex);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    protected override byte[] ProviderUnprotect(byte[] encryptedData)
    {
        return Protector.Unprotect(encryptedData);
    }

    /// <summary>
    /// Invokes a delegate where calls to <see cref="ProviderProtect(byte[])"/>
    /// and <see cref="ProviderUnprotect(byte[])"/> will ignore the primary
    /// purpose and instead use only the sub-purposes.
    /// </summary>
    public static byte[] RunWithSuppressedPrimaryPurpose(Func<object, byte[], byte[]> callback, object state, byte[] input)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        if (_suppressPrimaryPurpose)
        {
            return callback(state, input); // already suppressed - just forward call
        }

        try
        {
            try
            {
                _suppressPrimaryPurpose = true;
                return callback(state, input);
            }
            finally
            {
                _suppressPrimaryPurpose = false;
            }
        }
        catch
        {
            // defeat exception filters
            throw;
        }
    }
}
