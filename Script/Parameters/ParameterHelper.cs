using System;
using System.Collections.Generic;
using System.Linq; 
using Object = UnityEngine.Object;

namespace StateMachineSystem
{
static class ParameterHelper
{
    static readonly HashSet<ParameterComponent> parameters = new HashSet<ParameterComponent>();
    
    static HashSet<ParameterComponent> AllParameters<TFilter>() where TFilter : ParameterComponent
    {
        parameters.Clear();
        foreach (Object obj in Object.FindObjectsOfType(typeof(ParameterComponent)))
        {
            if (obj is TFilter param)
                parameters.Add(param);
        }         
        return parameters;
    }  

    internal static IReadOnlyCollection<ParameterComponent> GetParameters() => AllParameters<ParameterComponent>();
    
    internal static List<ParameterComponent> GetParametersSorted<TFilter>() where TFilter : ParameterComponent
    {
        List<ParameterComponent> sorted = AllParameters<TFilter>().ToList();
        sorted.Sort(ParameterSorting);
        return sorted;
    }
 
    public static ParameterTree GetSceneParameterTree() => new ParameterTree(GetParameters());

    static int ParameterSorting(ParameterComponent x, ParameterComponent y)
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