// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public class HttpFileCollectionWrapper : HttpFileCollectionBase
{
    private readonly HttpFileCollection _collection;

    public HttpFileCollectionWrapper(HttpFileCollection httpFileCollection)
    {
        ArgumentNullException.ThrowIfNull(httpFileCollection);

        _collection = httpFileCollection;
    }

    public override string[] AllKeys => _collection.AllKeys;

    public override int Count => _collection.Count;

    public override HttpPostedFileBase? this[string name] => Get(name);

    public override HttpPostedFileBase? this[int index] => Get(index);

    public override HttpPostedFileBase? Get(string name) =>
        _collection[name] is { } file ? new HttpPostedFileWrapper(file) : null;

    public override HttpPostedFileBase? Get(int index) =>
        _collection[index] is { } file ? new HttpPostedFileWrapper(file) : null;

    public override IList<HttpPostedFileBase> GetMultiple(string name)
         => _collection.GetMultiple(name)
            .Select(x => (HttpPostedFileBase)new HttpPostedFileWrapper(x))
            .ToList()
            .AsReadOnly();

    public override IEnumerator GetEnumerator() => _collection.GetEnumerator();
}
