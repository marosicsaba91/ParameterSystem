using System;
using UnityEngine;

namespace PlayBox
{
public class LocalParameterAttribute : PropertyAttribute
{
    public string defaultName;

    public LocalParameterAttribute(string defaultName)
    {
        this.defaultName = defaultName;
    }

    public LocalParameterAttribute()
    {
    }
}
}