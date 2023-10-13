using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace MachineKeyExample
{
    public static class MachineKeyTest
    {
        public const string AppName = "MachineKeyExample";

        private enum Action
        {
            None,
            Protect,
            Unprotect
        }

        private static string[] GetPurposes(HttpRequest request)
        {
            if (request.Form["purposes"] is { } purposes)
            {
                return purposes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
            }

            return Array.Empty<string>();
        }

        public static void Run(HttpContext context)
        {
            var purposes = GetPurposes(context.Request);
            var inputData = context.Request.Form["data"];
            Enum.TryParse<Action>(context.Request.Form["action"], ignoreCase: true, out var inputAction);

            context.Response.Write($"""
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" integrity="sha384-rbsA2VBKQhggwzxH7pPCaAqO46MgnOM80zW1RWuH61DGLwZJEdK2Kadq2F9CUG65" crossorigin="anonymous">

                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">System.Web.Security.MachineKey Example</h5>
                        <h6 class="card-subtitle mb-2 text-body-secondary">{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}</h4>
                        <form action="/" method="post" enctype="application/x-www-form-urlencoded">
                            <div class="mb-3">
                                <label for="purposes">Purposes (comma-delimited):</label>
                                <input type="text" name="purposes" class="form-control" value="{string.Join(",", purposes)}">
                            </div>
                            <div class="mb-3">
                                <label for="protected">Data to protect/unprotect:</label>
                                <input type="text" name="data" class="form-control" value="{inputData}">
                            </div>
                            <button type="submit" class="btn btn-primary" value="protect" name="action">Protect</button>
                            <button type="submit" class="btn btn-primary" value="unprotect" name="action">Unprotect</button>
                        </form>
                    </div>
                </div>
                """);

            if (inputAction == Action.Unprotect)
            {
                string content;

                try
                {
                    var bytes = Convert.FromBase64String(inputData);

                    var unprotected = MachineKey.Unprotect(bytes, purposes);
                    var str = Encoding.UTF8.GetString(unprotected);

                    content = $"""
                        <p>
                            Protected: <br><code>{inputData}</code>
                        </p>
                        <p>
                            Unprotected: <br><code>{str}</code>
                        </p>
                        """;
                }
                catch (Exception ex)
                {
                    content = $"""
                        <div class="alert alert-danger" role="alert">
                            Invalid input for <code>MachineKey.Unprotect</code>
                            <pre>{ex}</pre>
                        </div>
                    """;
                }

                context.Response.Write($"""
                        <div class="card">
                            <div class="card-body">
                                <h5 class="card-title">MachineKey.Unprotect</h5>
                                {content}
                            </div>
                        </div>
                    """);

            }

            if (inputAction == Action.Protect)
            {
                var @protected = MachineKey.Protect(Encoding.UTF8.GetBytes(inputData), purposes);
                var str = Convert.ToBase64String(@protected);

                context.Response.Write($"""
                        <div class="card">
                            <div class="card-body">
                                <h5 class="card-title">MachineKey.Protect</h5>
                                <p>
                                    Unprotected: <br><code>{inputData}</code>
                                </p>
                                <p>
                                    Protected: <br><code>{str}</code>
                                </p>
                            </div>
                        </div>
                    """);
            }
        }
    }
}
