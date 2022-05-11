using System.IO;

namespace ClassLibrary
{
    public static class SharedAuthUtils
    {
        public const string ApplicationName = "SystemWebAdaptersDemo";

        public const string AuthenticationScheme = "ApplicationCookie";

        public static DirectoryInfo SharedAuthDataProtectionDir
        {
            get
            {
                var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), ApplicationName));
                if (!dir.Exists)
                {
                    dir.Create();
                }

                return dir;
            }
        }
    }
}
