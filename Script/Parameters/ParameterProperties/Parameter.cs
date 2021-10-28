using System;
using UnityEngine;

namespace StateMachineSystem
{


[Serializable]
public abstract class Parameter
{
    internal abstract ParameterComponent ParameterComponent { get; set; }
    public abstract Type ComponentType { get; }
}


[Serializable]
public abstract class Parameter<TComponent> : Parameter where TComponent : ParameterComponent
{
    [SerializeField] internal TComponent parameterComponent;

    internal override ParameterComponent ParameterComponent
    {
        get => parameterComponent;
        set
        {
            if (parameterComponent == value) return;
            parameterComponent.UnSubscribe(this);

            parameterComponent = (TComponent)value;
            parameterComponent.Subscribe(this);
            OnParameterComponentChanged();
        }
    }

    protected virtual void OnParameterComponentChanged() { }

    public override Type ComponentType => typeof(TComponent);
}

[Serializable] public class TriggerParameter : Parameter<TriggerComponent>
{
    public void OnTriggered() { }
}


[Serializable]
public abstract class ValueParameter<T, TComponent, TParameter> : Parameter<TComponent> 
    where TComponent : ValueComponent<T, TParameter>
    where TParameter : Parameter
{
    [SerializeField] T ownValue;
    public event Action<T, T> valueChanged;
 

    public T Value
    {
        get => parameterComponent == null? ownValue : parameterComponent.Value;
        set
        { 
            T old = Value;
            if(value .Equals(old)) return;
             
            if ( parameterComponent != null)
            {
                if (parameterComponent.isSettingEnabled)
                    parameterComponent.Value = value;
            }
            else
                ownValue = value; 
            valueChanged?.Invoke(old, value);
        }
    }

    internal Type ValueType => typeof(T);
}

[Serializable] public class BoolParameter : ValueParameter<bool, BoolComponent, BoolParameter> { }
[Serializable] public class IntParameter : ValueParameter<int, IntComponent, IntParameter> { }
[Serializable] public class FloatParameter : ValueParameter<float, FloatComponent, FloatParameter> { }
[Serializable] public class StringParameter : ValueParameter<string, StringComponent, StringParameter> { }
[Serializable] public class Vector2Parameter : ValueParameter<Vector2, Vector2Component, Vector2Parameter> { }
[Serializable] public class Vector3Parameter : ValueParameter<Vector3, Vector3Component, Vector3Parameter> { }

}