// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace System.Web;

[SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = Constants.ApiFromAspNet)]
public sealed class HttpFileCollection : NameObjectCollectionBase
{
    private string[]? _keys;

    internal HttpFileCollection(IFormFileCollection files)
    {
        FormFiles = files;

        IsReadOnly = true;
    }

    internal IFormFileCollection FormFiles { get; }

    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public string[] AllKeys => _keys ??= GetKeys();

    public override int Count => FormFiles.Count;

    [Obsolete("Retrieving Keys is not supported on .NET 6+. Please use the enumerator instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override KeysCollection Keys => throw new PlatformNotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public override IEnumerator GetEnumerator()
    {
        foreach (var file in FormFiles)
        {
            yield return file.Name;
        }
    }

    public HttpPostedFile? Get(string name) => FormFiles.GetFile(name) is { } file ? new(file) : null;

    public IList<HttpPostedFile> GetMultiple(string name) => FormFiles.GetFiles(name) is { Count: > 0 } files ? new ReadOnlyPostedFileCollection(files) : Array.Empty<HttpPostedFile>();

    public HttpPostedFile? this[string name] => Get(name);

    private string[] GetKeys()
    {
        if (FormFiles.Count == 0)
        {
            return Array.Empty<string>();
        }

        var keys = new string[FormFiles.Count];
        var i = 0;

        foreach (var item in FormFiles)
        {
            keys[i++] = item.Name;
        }

        return keys;
    }

    /// <summary>
    /// Class used to wrap an <see cref="IReadOnlyList{T}"/> as an <see cref="IList{T}"/>. The API on System.Web would return a readonly <see cref="IList{T}"/>, so this mimics that behavior
    /// without needing to copy things to a new list.
    /// </summary>
    private class ReadOnlyPostedFileCollection : IList<HttpPostedFile>
    {
        private const string Message = "Collection is readonly";

        private readonly IReadOnlyList<IFormFile> _other;

        public ReadOnlyPostedFileCollection(IReadOnlyList<IFormFile> other)
        {
            _other = other;
        }

        public HttpPostedFile this[int index]
        {
            get => Create(_other[index]);
            set => throw new NotSupportedException(Message);
        }

        private static HttpPostedFile Create(IFormFile file) => new(file);

        public int Count => _other.Count;

        public bool IsReadOnly => true;

        public void Add(HttpPostedFile item) => throw new NotSupportedException(Message);

        public void Clear() => throw new NotSupportedException(Message);

        public bool Contains(HttpPostedFile item)
        {
            foreach (var other in _other)
            {
                if (Equals(other, item.File))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(HttpPostedFile[] array, int arrayIndex)
        {
            foreach (var item in _other)
            {
                array[arrayIndex++] = Create(item);
            }
        }

        public IEnumerator<HttpPostedFile> GetEnumerator()
        {
            foreach (var other in _other)
            {
                yield return Create(other);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(HttpPostedFile item)
        {
            var index = 0;

            foreach (var other in _other)
            {
                if (Equals(other, item.File))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void Insert(int index, HttpPostedFile item) => throw new NotSupportedException(Message);

        public bool Remove(HttpPostedFile item) => throw new NotSupportedException(Message);

        public void RemoveAt(int index) => throw new NotSupportedException(Message);
    }
}
