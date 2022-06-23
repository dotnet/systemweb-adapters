using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;

namespace OIDCAuth.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            // Send an OpenID Connect sign-in request.
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            // This has been updated to redirect back to the ASP.NET Core app in our incremental migration scenario
            // rather than redirecting back to the ASP.NET app directly.
            string callbackUrl = $"{GetScheme(HttpContext.Request)}://{GetHost(HttpContext.Request)}/Account/SignOutCallback";

            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }

        public ActionResult SignOutCallback()
        {
            if (Request.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        private static string GetHost(HttpRequestBase request) =>
            request.Headers["x-forwarded-host"]
                ?? (request.Url.IsDefaultPort ? request.Url.Host : $"{request.Url.Host}:{request.Url.Port}");

        private static string GetScheme(HttpRequestBase request) =>
            request.Headers["x-forwarded-proto"] ?? request.Url.Scheme;
    }
}
