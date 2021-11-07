using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayBox
{

[Serializable]
public class Parameter
{
    public enum Type
    {
        Value,
        Variable
    }

    public Type parameterType = Type.Value;
    public virtual object Value { get; set; }
}

[Serializable]
public abstract class Parameter<T, TVariable> : Parameter where TVariable : ValueVariable<T>
{
    [SerializeField] TVariable variable;
    [SerializeField] T value;

    public override object Value
    {
        get
        {
            if (parameterType == Type.Value)
                return value;
            if (parameterType == Type.Variable && variable != null)
                return variable.Value;
            return default;
        }
        set
        {
            if (parameterType != Type.Value) return;
            this.value = (T)value;
        }
    }
}


[Serializable]
public abstract class ArrayParameter<T, TVariable, TParameter> : Parameter
    where TVariable : ValueVariable<T>
    where TParameter : Parameter<T, TVariable>
{ 
    [SerializeField] List<TParameter> parameters;
}
}