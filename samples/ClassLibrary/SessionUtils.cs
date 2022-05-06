using Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

namespace ClassLibrary;

public class SessionUtils
{
    public static string ApiKey = "test-key";

    public static void RegisterSessionKeys(SessionSerializerOptions options)
    {
        options.RegisterKey<int>("test-value");
        options.RegisterKey<SessionDemoModel>("SampleSessionItem");
    }
}
