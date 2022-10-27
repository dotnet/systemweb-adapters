// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETFRAMEWORK

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

/// <summary>
/// Helper methods for <see cref="BinaryWriter"/> and <see cref="BinaryReader"/> that aren't available on .NET Framework.
/// </summary>
internal static class BinaryWriterReaderExtensions
{
    /// <see href="https://source.dot.net/#System.Private.CoreLib/BinaryWriter.cs,2daa1d14ff1877bd"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
    {
        var uValue = (uint)value;

        // Write out an int 7 bits at a time. The high bit of the byte,
        // when on, tells reader to continue reading more bytes.
        //
        // Using the constants 0x7F and ~0x7F below offers smaller
        // codegen than using the constant 0x80.

        while (uValue > 0x7Fu)
        {
            writer.Write((byte)(uValue | ~0x7Fu));
            uValue >>= 7;
        }

        writer.Write((byte)uValue);
    }

    /// <see href="https://source.dot.net/#System.Private.CoreLib/BinaryReader.cs,f30b8b6e8ca06e0f"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Read7BitEncodedInt(this BinaryReader reader)
    {
        // Unlike writing, we can't delegate to the 64-bit read on
        // 64-bit platforms. The reason for this is that we want to
        // stop consuming bytes if we encounter an integer overflow.

        uint result = 0;
        byte byteReadJustNow;

        // Read the integer 7 bits at a time. The high bit
        // of the byte when on means to continue reading more bytes.
        //
        // There are two failure cases: we've read more than 5 bytes,
        // or the fifth byte is about to cause integer overflow.
        // This means that we can read the first 4 bytes without
        // worrying about integer overflow.

        const int MaxBytesWithoutOverflow = 4;
        for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
        {
            // ReadByte handles end of stream cases for us.
            byteReadJustNow = reader.ReadByte();
            result |= (byteReadJustNow & 0x7Fu) << shift;

            if (byteReadJustNow <= 0x7Fu)
            {
                return (int)result; // early exit
            }
        }

        // Read the 5th byte. Since we already read 28 bits,
        // the value of this byte must fit within 4 bits (32 - 28),
        // and it must not have the high bit set.

        byteReadJustNow = reader.ReadByte();
        if (byteReadJustNow > 0b_1111u)
        {
            throw new FormatException("Invalid sequence");
        }

        result |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
        return (int)result;
    }
}

#endif
