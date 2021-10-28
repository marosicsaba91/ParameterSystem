
using System;
using System.Collections.Generic;

namespace StateMachineSystem
{
class ParameterTree
{
    public string categoryName;
    public ParameterTree parent;
    public List<ParameterComponent> parameters;
    public SortedDictionary<string, ParameterTree> childCategories;

    public ParameterTree(IEnumerable<ParameterComponent> allParams)
    {
        childCategories = new SortedDictionary<string, ParameterTree>();
        parameters = new List<ParameterComponent>();
        parent = null;
        categoryName = string.Empty;
        
        foreach (ParameterComponent param in allParams)
        {
            IEnumerable<string> paramCategories = param.Path;
            ParameterTree childCategory = this;
            foreach (string category in paramCategories)
                childCategory = childCategory.GetChild(category);
            childCategory.parameters.Add(param);
        }

        SortParametersRecursively();
    }

    void SortParametersRecursively()
    {
        parameters.Sort(ParameterSorting);
        if(childCategories == null) return;
        foreach (KeyValuePair<string, ParameterTree> nodes in childCategories)
            nodes.Value.SortParametersRecursively();
    }

    int ParameterSorting(ParameterComponent x, ParameterComponent y) => 
        string.Compare(x.name, y.name, StringComparison.Ordinal);
    

    public ParameterTree(string categoryName, ParameterTree parent)
    {
        this.categoryName = categoryName;
        this.parent = parent;
        childCategories = new SortedDictionary<string, ParameterTree>();
        parameters = new List<ParameterComponent>();
    }

    ParameterTree GetChild(string category)
    {
        if (childCategories == null)
            childCategories = new SortedDictionary<string, ParameterTree>();
        if (childCategories.ContainsKey(category))
            return childCategories[category];

        var child = new ParameterTree(category, this);
        childCategories.Add(category, child);
        return child;
    }

    public IEnumerable<string> Categories()
    {
        yield return categoryName;
        if (parent == null)
            yield break;
        foreach (string category in parent.Categories())
            yield return category; 
    }
}
}