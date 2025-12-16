using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity;

void CheckType(Type type, string typeName) {
    Console.WriteLine($"=== {typeName} ===");
    Console.WriteLine($"IsClass: {type.IsClass}");
    Console.WriteLine($"IsGenericType: {type.IsGenericType}");
    Console.WriteLine($"IsAbstract: {type.IsAbstract}");
    Console.WriteLine($"BaseType: {type.BaseType?.Name}");
    Console.WriteLine();

    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    Console.WriteLine("Constructors:");
    foreach (var ctor in ctors) {
        var parms = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"  {ctor.Name}({parms})");
    }
    Console.WriteLine();

    var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    Console.WriteLine("Properties:");
    foreach (var prop in props) {
        var getter = prop.GetGetMethod()?.IsPublic == true ? "get;" : "";
        var setter = prop.GetSetMethod()?.IsPublic == true ? " set;" : "";
        var setterVisibility = "";
        if (prop.GetSetMethod() == null && prop.GetSetMethod(true) != null) {
            var nonPublicSetter = prop.GetSetMethod(true);
            if (nonPublicSetter.IsPrivate) setterVisibility = " private set;";
            else if (nonPublicSetter.IsFamily) setterVisibility = " protected set;";
            else if (nonPublicSetter.IsAssembly) setterVisibility = " internal set;";
        }
        Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name} {{ {getter}{setter}{setterVisibility} }}");
    }
    Console.WriteLine();
}

CheckType(typeof(IdentityUserClaim<string>), "IdentityUserClaim<string>");
CheckType(typeof(IdentityUserLogin<string>), "IdentityUserLogin<string>");
CheckType(typeof(IdentityUserRole<string>), "IdentityUserRole<string>");
CheckType(typeof(IdentityUserToken<string>), "IdentityUserToken<string>");
CheckType(typeof(IdentityRoleClaim<string>), "IdentityRoleClaim<string>");
