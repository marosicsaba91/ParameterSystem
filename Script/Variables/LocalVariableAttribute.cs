using System;
using UnityEngine;

namespace PlayBox
{
[AttributeUsage(AttributeTargets.Field)]
public class LocalVariableAttribute : PropertyAttribute
{
    public string defaultName;

    public LocalVariableAttribute(string defaultName)
    {
        this.defaultName = defaultName;
    }

    public LocalVariableAttribute()
    {
    }
}
}