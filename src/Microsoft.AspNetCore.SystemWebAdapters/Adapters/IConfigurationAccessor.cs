using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Adapters;

internal interface IConfigurationAccessor
{
    string? GetSetting(string key);

    string? GetConnectionString(string name);
}
