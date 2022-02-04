using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayBox
{
[Serializable]
public class InnerVariablesDashboard
{
    public int MaxItemCount => 10;
    public List<string> openedVariables = new List<string>();
    public Vector2 scrollPosition;
}
}