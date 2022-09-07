using System.Web;

namespace ClassLibrary;

public class RequestInfo
{
    public static void WriteRequestInfo(bool suppress)
    {
        var context = HttpContext.Current;

        context.Response.ContentType = "text/html";

        using (var writer = new SimpleJsonWriter(context.Response))
        {
            writer.Write("VirtualDirectory", HttpRuntime.AppDomainAppVirtualPath);
            writer.Write("PhysicalDirectory", HttpRuntime.AppDomainAppPath);
            writer.Write("RequestDirectory", context.Server.MapPath(null));
            writer.Write("RequestDirectory2", context.Server.MapPath(""));
            writer.Write("UploadedFiles", context.Server.MapPath("/UploadedFiles"));
            writer.Write("RelativeFiles", context.Server.MapPath("UploadedFiles"));
            writer.Write("AppFiles", context.Server.MapPath("~/MyUploadedFiles"));
            writer.Write("RequestVirtualDirectory", context.Request.ApplicationPath);
            writer.Write("RawUrl", context.Request.RawUrl);
            writer.Write("Path", context.Request.Path);
            writer.Write("Length", context.Request.InputStream.Length);
            writer.Write("Charset", context.Response.Charset);
            writer.Write("ContentType", context.Response.ContentType);
            writer.Write("ContentEncoding", context.Response.ContentEncoding);
            context.Response.Output.Flush();

            if (context.Session is { } state && state["test-value"] is int value)
            {
                writer.Write("test-value", value);
            }

            writer.Write("ContentType", context.Response.ContentType);
            writer.Write("ContentEncoding", context.Response.ContentEncoding.WebName);

            context.Response.ContentType = "application/json";
            writer.Write("ContentType", context.Response.ContentType);
            writer.Write("ContentEncoding", context.Response.ContentEncoding.WebName);

            // Status code
            writer.Write("StatusCode", context.Response.StatusCode);
            writer.Write("StatusDescription", context.Response.StatusDescription);
            context.Response.End();
        }

        context.Response.SuppressContent = suppress;
        context.Response.End();
    }
}
