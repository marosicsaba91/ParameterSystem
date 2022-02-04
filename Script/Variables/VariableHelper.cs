using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MarosiUtility;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif
using Object = UnityEngine.Object;

namespace PlayBox
{
static class VariableHelper
{
    const string openedVariablesEditorPrefsKey = "Opened Variables";

    class Savable : ScriptableObject
    {
        public List<string> openedVariables;

        public Savable()
        {
            openedVariables = new List<string>();
        }
    }

    static Savable _savable;

    public static List<string> OpenedVariables
    {
        get
        {
#if UNITY_EDITOR
            if (_savable == null)
            {
                _savable = ScriptableObject.CreateInstance<Savable>();
                string data = EditorPrefs.GetString(
                    openedVariablesEditorPrefsKey, JsonUtility.ToJson(_savable, prettyPrint: false));
                JsonUtility.FromJsonOverwrite(data, _savable);
            }
#endif
            if (_savable == null)
                _savable = ScriptableObject.CreateInstance<Savable>();

            return _savable.openedVariables;
        }
        set
        {
            if (_savable == null)
                _savable = ScriptableObject.CreateInstance<Savable>();

            _savable.openedVariables = value;

            string data = JsonUtility.ToJson(_savable, prettyPrint: false);
            EditorPrefs.SetString(openedVariablesEditorPrefsKey, data);
        }
    }


    static readonly bool debugLogs = false;
    static Dictionary<Type, Dictionary<string, MethodInfo>> _functions = null;
     
    public static Dictionary<string, MethodInfo> Functions(Type type)
    {
        FindPlayBoxFunctions();
        if (_functions.TryGetValue(type, out Dictionary<string, MethodInfo> result))
            return result;
        return null;
    }
     
    static void FindPlayBoxFunctions()
    {
        if(_functions!=null) return;
        DateTime start =DateTime.Now;

        var methodsFound = 0;
        _functions = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        const BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                                     BindingFlags.Static | BindingFlags.Instance;
        
        foreach (Assembly assembly in ReflectionHelper.FindRelevantAssemblies())
        {
            DateTime startAssembly =DateTime.Now;
            foreach (Type type in assembly.GetTypes()) foreach (MethodInfo method in type.GetMethods(binding))
            {
                var attribute = method.GetCustomAttribute<PlayBoxFunctionAttribute>();
                if (attribute == null) continue;

                Type returnType = method.ReturnType;
                if (returnType == typeof(void))
                {
                    Debug.LogWarning($"Method: {method.Name} can't be used as a PlayBox function without return type!");
                    continue;
                } 
                
                if (!method.IsStatic)
                {
                    Debug.LogWarning($"Method: {method.Name} can't be used as a PlayBox function because it's not static!");
                    continue;
                }
 
                string uniqMethodName = attribute.UniqName(method);

                if (!_functions.ContainsKey(returnType))
                    _functions.Add(returnType, new Dictionary<string, MethodInfo>());

                Dictionary<string, MethodInfo> dict = _functions[returnType];

                if (dict.ContainsKey(uniqMethodName))
                {
                    Debug.LogWarning($"Method name: {uniqMethodName} is already used as a PlayBox uniq function name for the {returnType} return type!");
                    continue;
                }
                dict.Add(uniqMethodName, method);
                methodsFound++;
            }
            
            DateTime endAssembly =DateTime.Now; 
            if(debugLogs)
                Debug.Log($"Variable Setup for Assembly: {assembly.GetName().Name}:     {(endAssembly - startAssembly ).TotalMilliseconds} ms"); 
        }

        DateTime end =DateTime.Now;
        if (debugLogs)
            Debug.Log($"All Variable Setup:     {(end - start).TotalMilliseconds} ms\nPlayBox functions found:     {methodsFound}");
    }

    internal static IEnumerable<Variable> AllVariables() => 
        Object.FindObjectsOfType(typeof(Variable)).Cast<Variable>();
    public static IEnumerable<Variable> AllGlobalVariables() => 
        AllVariables().Where(variable => variable.visibility == Variable.Visibility.Global);

    static IEnumerable<Variable> AllVariables<TFilter>() where TFilter : Variable
    { 
        foreach (Object obj in Object.FindObjectsOfType(typeof(Variable)))
        {
            if (obj is TFilter variable)
                yield return variable;
        }          
    }  
    
    static IEnumerable<Variable> AllVariables(Type filter) => 
        Object.FindObjectsOfType(filter).Cast<Variable>();
  
    internal static List<Variable> GetSortedVariables<TFilter>() where TFilter : Variable
    {
        List<Variable> sorted = AllVariables<TFilter>().ToList();
        sorted.Sort(VariableSorting);
        return sorted;
    }
    
    internal static List<Variable> GetSortedVariables(Type filter) 
    {
        List<Variable> sorted = AllVariables(filter).ToList();
        sorted.Sort(VariableSorting);
        return sorted;
    }

  

    static int VariableSorting(Variable x, Variable y)
    {
        IEnumerator<string> xCategories = x.Path.GetEnumerator(); 
        IEnumerator<string> yCategories = y.Path.GetEnumerator();   
        int result;
        while (true)
        {
            bool xe = xCategories.MoveNext();
            bool ye = yCategories.MoveNext();
            result = xe.CompareTo(ye);

            if (!xe && !ye)
                break;
            
            if (result != 0)
                break;
            
            result = string.Compare(xCategories.Current, yCategories.Current, StringComparison.Ordinal);
            if (result != 0)
                break;
        }
        xCategories.Dispose();
        yCategories.Dispose();
        if (result != 0)
            return result;
        
        result = string.Compare(x.name, y.name, StringComparison.Ordinal); 
        return result;
    }
    
 
}
}