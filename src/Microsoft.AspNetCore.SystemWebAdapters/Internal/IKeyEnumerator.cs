using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal;

internal interface IKeyEnumerator
{
    IEnumerable<string> Keys { get; }
}
