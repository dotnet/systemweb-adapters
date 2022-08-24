using System;
using System.Collections.Generic;

namespace ClassLibrary;

public class RemoteServiceUtils
{
    // Do not re-use this ApiKey; every solution should use a unique ApiKey
    public static string ApiKey = "54b69938-90dd-4f79-adcd-27fbd6f0e4b7";

    public static void RegisterSessionKeys(IDictionary<string, Type> knownTypes)
    {
        knownTypes.Add("test-value", typeof(int));
        knownTypes.Add("SampleSessionItem", typeof(SessionDemoModel));
    }
}
