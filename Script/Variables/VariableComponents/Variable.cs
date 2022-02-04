using System;
using System.Collections.Generic;
using System.Reflection;
using MUtility;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace PlayBox
{
public abstract class Variable : MonoBehaviour
{
    public enum Visibility { Object, Parent, Global }

    [FormerlySerializedAs("path")] [SerializeField] PathProperty pathString = new PathProperty(); 
    [FormerlySerializedAs("pathStrings")] [SerializeField, HideInInspector] string[] path = Array.Empty<string>();
    [SerializeField] public bool isGUISettingEnabled = true;
    [SerializeField] public List<Object> otherObjectChangingWithVariable;
    [SerializeField] public Visibility visibility = Visibility.Parent;
    
    /// <summary>
    /// Return the UnityEngine.Objects that can be changed by Changing the value of this Variable
    /// For Undo And Redo
    /// </summary>
    internal IEnumerable<Object> ChangingObjects
    {
        get
        {
            yield return this;
            if (otherObjectChangingWithVariable != null)
                foreach (Object otherObject in otherObjectChangingWithVariable)
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
    
    [Serializable]
    class PathProperty : InspectorProperty<Variable, string>
    {
        protected override void SetValue(Variable parentObject, string newValue)
        {
            parentObject.path = GetCategoriesFromString(newValue);
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
        
        protected override string GetLabel(Variable parentObject, string originalLabel) => "Path";
    }

    internal abstract Type ValueType { get; }

    // FUNCTION
    
    [SerializeField] string functionUniqName;
    [SerializeField] protected List<Parameter> functionParameters;

    Dictionary<string, MethodInfo> _availableFunctions = null;

    internal IReadOnlyDictionary<string, MethodInfo> AvailableFunctions =>
        _availableFunctions ?? (_availableFunctions = VariableHelper.Functions(ValueType));

    bool _functionsCached = false;
    MethodInfo _function = null;
    internal string FunctionUniqName => functionUniqName;

    void OnValidate()
    {
        _functionsCached = false;
    }
    

    internal MethodInfo SourceFunction
    {
        get
        {
            if (_functionsCached) return _function;

            if (!string.IsNullOrEmpty(functionUniqName) &&
                AvailableFunctions.TryGetValue(functionUniqName, out MethodInfo function))
                _function = function;
            else
                _function = null;
            _functionsCached = true;

            return _function;
        }
        set
        {
            
            if(AvailableFunctions == null) return;
            _functionsCached = false;
            if(value == null)
            {
                functionUniqName = null;
                return;
            }
            foreach (KeyValuePair<string, MethodInfo> function in AvailableFunctions)
            {
                if (function.Value != value) continue;
                functionUniqName = function.Key;
                return;
            }
        }
    }

    public bool HasValidSource => string.IsNullOrEmpty(functionUniqName) || SourceFunction != null;

    internal Parameter GetParameterAt(int index)
    {
        if (functionParameters == null) return null;
        if (functionParameters.Count < index + 1) return null;
        if (index < 0) return null;
        return functionParameters[index];
    }
    
    internal void SetParameterAt(int index, Parameter parameter)
    {
        if (functionParameters == null) functionParameters = new List<Parameter>(index);

        while (functionParameters.Count < index + 1)
            functionParameters.Add(null);

        functionParameters[index] = parameter;
    }
}
}