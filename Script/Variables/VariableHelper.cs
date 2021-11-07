using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayBox
{
static class VariableHelper
{
    
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

        int methodsFound = 0;
        _functions = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        var currentAssembly = Assembly.GetExecutingAssembly();
        string currentAssemblyFullName = currentAssembly.FullName;
        const string globalAssembly = "Assembly-CSharp";
        const BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
                                     BindingFlags.Static | BindingFlags.Instance;
        
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            DateTime startAssembly =DateTime.Now;
            string assemblyName = assembly.GetName().Name;
            bool isCurrent = assembly == currentAssembly; 
            bool isGlobal = assemblyName == globalAssembly;
            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            bool isReferencingCurrent =
                referencedAssemblies.Select(an => an.FullName).Contains(currentAssemblyFullName);
            
            if (!isCurrent && !isGlobal && !isReferencingCurrent)
                continue; 
            
            
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
    internal static IEnumerable<Variable> ShownVariables() => 
        AllVariables().Where(variable => variable.showOnDashboard);

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

 
    public static VariableTree GetVariableTree() => new VariableTree(ShownVariables());

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