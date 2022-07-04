using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayBox
{
public class BlackBoard : MonoBehaviour
{ 
    [SerializeField] GameObjectScope scope;
    
    internal List<string> openedVariables = new List<string>();
    internal Vector2 scrollPosition;

    List<Variable> _variables;

    public IEnumerable<Variable> Variables => _variables ?? (_variables = FindVariables());

    List<Variable> FindVariables()
    {
        return new List<Variable>();
    }
}


[Flags]
public enum GameObjectScope
{
    Self = 1,
    Children = 2,
    Parents = 4,
    Siblings = 8
}

public static class GameObjectScopeHelper
{
    public static IEnumerable<Transform> GetAllGameObjectInsideScope(this GameObjectScope scope, Transform self)
    {
        if ((scope & GameObjectScope.Self) != 0)
            yield return self;
        if ((scope & GameObjectScope.Children) != 0)
            for (int i = 0; i < self.childCount; i++)
                yield return self.GetChild(i);
        
        Transform parent = self.parent;
        if (parent == null)
        {
            // RETURN OTHER ROOT OBJECTS
            yield break;
        }

        if ((scope & GameObjectScope.Siblings) != 0)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform sibling = parent.GetChild(i);
                if (sibling != self) 
                    yield return parent.GetChild(i);
            }
        }
        
        if ((scope & GameObjectScope.Parents) != 0)
            while (parent != null)
            {
                yield return parent;
                parent = parent.parent;
            }
    }
}

}