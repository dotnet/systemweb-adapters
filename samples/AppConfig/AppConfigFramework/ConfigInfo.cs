using System;
using System.Linq;
using System.Text.Json;
using System.Web;
using System.Web.SessionState;

namespace AppConfigFramework;

public class ConfigInfo : IHttpHandler, IRequiresSessionState
{
    public bool IsReusable => true;

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 200;

        context.Response.Write(JsonSerializer.Serialize(new
        {
            Setting1 = AppConfiguration.GetSetting("Setting1"),
            Setting2 = AppConfiguration.GetSetting("Setting2"),
            ConnStr1 = AppConfiguration.GetConnectionString("ConnStr1"),
            ConnStr2 = AppConfiguration.GetConnectionString("ConnStr2")
        }));
    }
}
