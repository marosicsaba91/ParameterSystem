using System;
using System.Collections.Generic;
using MUtility;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace PlayBox
{
public abstract class Parameter : MonoBehaviour
{
    [FormerlySerializedAs("path")] [SerializeField] PathProperty pathString = new PathProperty(); 
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
            return p == string.Empty ? name : name + " : " + p;
        }
    }
    
    [Serializable]
    class PathProperty : InspectorProperty<Parameter, string>
    {
        protected override void SetValue(Parameter parentObject, string value)
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
        
        protected override string Text(Parameter parentObject, string originalLabel) => "Path";
    }
}

public abstract class ValueParameter<T> : Parameter
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