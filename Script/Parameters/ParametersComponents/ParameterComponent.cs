using System;
using System.Collections.Generic;
using MUtility;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace StateMachineSystem
{
public abstract class ParameterComponent : MonoBehaviour
{ 
    [FormerlySerializedAs("path")] [SerializeField] PathProperty pathString; 
    [FormerlySerializedAs("pathStrings")] [SerializeField, HideInInspector] string[] path = Array.Empty<string>();
    [SerializeField] public bool isSettingEnabled = true;
    [SerializeField] public List<Object> otherObjectChangingWithParameter;

    /// <summary>
    /// Return the UnityEngine.Objects that can be changed by Changing the value of this Parameter
    /// For Undo And Redo
    /// </summary>

    internal IEnumerable<Object> ChangingObjects
    {
        get
        {
            yield return this;
            if (otherObjectChangingWithParameter != null)
                foreach (Object otherObject in otherObjectChangingWithParameter)
                    yield return otherObject;
        }
    }

    /// <summary>
    /// For Easier navigation in Editor.
    /// </summary>
    public IEnumerable<string> Path => path;

    public string PathString
    {
        get => pathString.Value;
        set => pathString.SetValue(this, value);
    }  
    
    public string FullPathString
    {
        get
        {
            string p = pathString.Value;
            return p == string.Empty ? name : p + " / " + name;
        }
    }

    internal abstract void Subscribe(Parameter parameter);
    internal abstract void UnSubscribe(Parameter parameter);

    [Serializable]
    class PathProperty : InspectorProperty<ParameterComponent, string>
    {
        protected override void SetValue(ParameterComponent parentObject, string value)
        {
            parentObject.path = GetCategoriesFromString(value);
            base.SetValue(parentObject, GetCategoriesStringCategories(parentObject.path));
        }

        static string[] GetCategoriesFromString(string categories)
        {
            if (string.IsNullOrEmpty(categories))
                return Array.Empty<string>();
            string[] split = categories.Split('\\', '/');
            var result = new List<string>();
            foreach (string t in split)
            {
                string newString = t.Trim();
                if (newString != string.Empty)
                    result.Add(newString);
            }

            return result.ToArray();
        }

        static string GetCategoriesStringCategories(IEnumerable<string> categories) =>
            string.Join(" / ", categories);
        
        protected override string Text(ParameterComponent parentObject, string originalLabel) => "Path";
    }
}

public abstract class ParameterComponent<TParameter> : ParameterComponent where TParameter : Parameter
{
    [SerializeField] internal List<TParameter> subscribers = new List<TParameter>();
    internal override void Subscribe(Parameter subscriber) => subscribers.Add((TParameter)subscriber);
    internal override void UnSubscribe(Parameter subscriber) => subscribers.Remove((TParameter)subscriber);
}

public abstract class ValueComponent<T, TParameter> : ParameterComponent<TParameter> where TParameter : Parameter
{
    [SerializeField] T value;
     
    public T Value
    {
        get => value;
        set
        {
            if(Equals(value, this.value)) return;
            T old = this.value;
            this.value = value;
            valueChanged?.Invoke(old, value);
        }
    }

    public event Action<T, T> valueChanged;
} 
}