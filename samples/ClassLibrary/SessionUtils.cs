using System;
using System.Collections.Generic;

namespace ClassLibrary;

public class SessionUtils
{
    public static string ApiKey = "test-key";

    public static void RegisterSessionKeys(IDictionary<string, Type> knownTypes)
    {
        knownTypes.Add("test-value", typeof(int));
        knownTypes.Add("SampleSessionItem", typeof(SessionDemoModel));
    }
}
