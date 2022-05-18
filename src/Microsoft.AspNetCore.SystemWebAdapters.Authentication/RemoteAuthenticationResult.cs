using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.AspNetCore.SystemWebAdapters.Authentication;

public class RemoteAuthenticationResult
{
    // TODO : Document these properties and in what circumstances
    //        each should be used.
    public ClaimsPrincipal? User { get; set; }
    public int StatusCode { get; set; }
    public IDictionary<string, IEnumerable<string>> ResponseHeaders { get; set; } = new Dictionary<string, IEnumerable<string>>();
    public ResponseContent? Content { get; set; }

    public class ResponseContent
    {
        public string ContentType { get; set; }
        public byte[] Content { get; set; }

        public ResponseContent(string contentType, byte[] content)
        {
            ContentType = contentType;
            Content = content;
        }
    }
}
