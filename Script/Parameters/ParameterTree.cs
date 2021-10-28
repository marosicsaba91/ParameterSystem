
using System;
using System.Collections.Generic;

namespace PlayBox
{
class ParameterTree
{
    public string categoryName;
    public ParameterTree parent;
    public List<Parameter> parameters;
    public SortedDictionary<string, ParameterTree> childCategories;

    public ParameterTree(IEnumerable<Parameter> allParams)
    {
        childCategories = new SortedDictionary<string, ParameterTree>();
        parameters = new List<Parameter>();
        parent = null;
        categoryName = string.Empty;
        
        foreach (Parameter param in allParams)
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

    int ParameterSorting(Parameter x, Parameter y) => 
        string.Compare(x.name, y.name, StringComparison.Ordinal);
    

    public ParameterTree(string categoryName, ParameterTree parent)
    {
        this.categoryName = categoryName;
        this.parent = parent;
        childCategories = new SortedDictionary<string, ParameterTree>();
        parameters = new List<Parameter>();
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