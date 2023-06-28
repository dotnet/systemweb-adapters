using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModulesFramework
{
    public class Handler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
        }
    }
}
