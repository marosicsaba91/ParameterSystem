using System;
using System.Reflection;

namespace PlayBox
{

[AttributeUsage(AttributeTargets.Method)]
public class PlayBoxFunctionAttribute : Attribute
{
    readonly string _uniqName;
    readonly string _displayName;
    readonly string _shortName;
    

    public PlayBoxFunctionAttribute() { }

    public PlayBoxFunctionAttribute(string uniqName, string displayName, string shortName)
    {
        _uniqName = uniqName;
        _displayName = displayName;
        _shortName = shortName;
    }

    
    public string UniqName(MethodInfo method) => Name(method, _uniqName);

    public string DisplayName(MethodInfo method) => Name(method, _displayName);

    public string ShortName(MethodInfo method) => Name(method, _shortName);

    static string Name(MemberInfo method, string name)
    {
        if (string.IsNullOrEmpty(name))
            return method.Name;
        return name;
    }
} 
}