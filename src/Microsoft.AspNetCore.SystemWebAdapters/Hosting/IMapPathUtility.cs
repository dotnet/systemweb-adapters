namespace System.Web.Hosting;

internal interface IMapPathUtility
{
    string MapPath(string requestPath, string? path);
}
