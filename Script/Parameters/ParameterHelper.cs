using System;
using System.Collections.Generic;
using System.Linq; 
using Object = UnityEngine.Object;

namespace PlayBox
{
static class ParameterHelper
{ 
    internal static IEnumerable<Parameter> AllParameters() => 
        Object.FindObjectsOfType(typeof(Parameter)).Cast<Parameter>();

    static IEnumerable<Parameter> AllParameters<TFilter>() where TFilter : Parameter
    { 
        foreach (Object obj in Object.FindObjectsOfType(typeof(Parameter)))
        {
            if (obj is TFilter param)
                yield return param;
        }          
    }  
    
    static IEnumerable<Parameter> AllParameters(Type filter) => 
        Object.FindObjectsOfType(filter).Cast<Parameter>();
  
    internal static List<Parameter> GetParametersSorted<TFilter>() where TFilter : Parameter
    {
        List<Parameter> sorted = AllParameters<TFilter>().ToList();
        sorted.Sort(ParameterSorting);
        return sorted;
    }
    
    internal static List<Parameter> GetParametersSorted(Type filter) 
    {
        List<Parameter> sorted = AllParameters(filter).ToList();
        sorted.Sort(ParameterSorting);
        return sorted;
    }

 
    public static ParameterTree GetSceneParameterTree() => new ParameterTree(AllParameters());

    static int ParameterSorting(Parameter x, Parameter y)
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