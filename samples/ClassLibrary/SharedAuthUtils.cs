using System.IO;

namespace ClassLibrary
{
    public static class SharedAuthUtils
    {
        public const string ApplicationName = "SystemWebAdaptersDemo";

        public static DirectoryInfo SharedAuthDataProtectionDir =>
            new(Path.Combine(Path.GetTempPath(), ApplicationName));
    }
}
