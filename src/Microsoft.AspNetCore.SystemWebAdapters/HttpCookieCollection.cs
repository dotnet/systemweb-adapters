// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public sealed class HttpCookieCollection : NameObjectCollectionBase
{
    public HttpCookieCollection()
    {
    }

    internal HttpCookieCollection(HttpRequestCore request)
    {
        foreach (var (name, value) in request.Cookies)
        {
#pragma warning disable CA5396
            Add(new HttpCookie(name, value));
#pragma warning restore CA5396
        }
    }

    internal HttpCookieCollection(HttpResponse response)
    {
        response.UnwrapAdapter().OnStarting(static state =>
        {
            var response = (HttpResponse)state;
            var cookies = response.UnwrapAdapter().Cookies;

            for (var i = 0; i < response.Cookies.Count; i++)
            {
                if (response.Cookies[i] is { } cookie)
                {
                    cookies.Append(cookie.Name, cookie.Value ?? string.Empty, cookie.ToCookieOptions());
                }
            }

            return Task.CompletedTask;
        }, response);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public string?[] AllKeys => BaseGetAllKeys();

    public HttpCookie? this[string name] => Get(name);

    public HttpCookie? this[int index] => Get(index);

    public void Add(HttpCookie cookie)
    {
        if (cookie is null)
        {
            throw new ArgumentNullException(nameof(cookie));
        }

        BaseAdd(cookie.Name, cookie);
    }

    public void Set(HttpCookie cookie)
    {
        if (cookie is null)
        {
            throw new ArgumentNullException(nameof(cookie));
        }

        BaseSet(cookie.Name, cookie);
    }

    public HttpCookie? Get(string name) => (HttpCookie?)BaseGet(name);

    public HttpCookie? Get(int index) => (HttpCookie?)BaseGet(index);

    public string? GetKey(int index) => BaseGetKey(index);

    public void Remove(string name) => BaseRemove(name);

    public void Clear() => BaseClear();
}
