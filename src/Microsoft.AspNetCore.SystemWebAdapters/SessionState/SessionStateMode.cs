// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.SessionState;

public enum SessionStateMode
{
    Off = 0,
    InProc = 1,
    StateServer = 2,
    SQLServer = 3,
    Custom = 4
};
