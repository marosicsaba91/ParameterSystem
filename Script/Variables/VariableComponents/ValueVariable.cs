using System;
using System.Collections.Generic; 
using System.Reflection;
using UnityEngine; 

namespace PlayBox
{
[Serializable]
public abstract class ValueVariable<T> : Variable
{
    [SerializeField] T value;

    public T Value
    {
        get
        {
            if (SourceFunction == null)
                return value;
            if (functionParameters == null)
                functionParameters = new List<Parameter>();
            return default;
        }
        set
        {
            if (Equals(value, this.value)) return;
            T old = this.value;
            this.value = value;
            valueChanged?.Invoke(old, value);
        }
    }

    public event Action<T, T> valueChanged;


    internal override Type ValueType => typeof(T);

    [Serializable]
    internal class Param : Parameter<T, ValueVariable<T>> { }
    
    [Serializable]
    internal class ArrayParam : ArrayParameter<T, ValueVariable<T>, Param> { }

    internal Parameter CreateNewParameter() => new Param();
    internal Parameter CreateNewArrayParameter() => new ArrayParam();
}
}