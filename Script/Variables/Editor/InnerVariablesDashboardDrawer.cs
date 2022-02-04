using System.Collections.Generic;
using System.Linq;
using MUtility;
using UnityEditor;
using UnityEngine;

namespace PlayBox
{
[CustomPropertyDrawer(typeof(InnerVariablesDashboard))]
public class InnerVariablesDashboardDrawer : PropertyDrawer
{
    static float _itemHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetupHeight();
        var dashboard = (InnerVariablesDashboard)property.GetObjectOfProperty(); 
        if (!TryGetVariables(property, out IEnumerable<Variable> variables))
            return;
 
        Rect headerPos = position;
        headerPos.height = _itemHeight;
        property.isExpanded = EditorGUI.Foldout(headerPos, property.isExpanded, label);
        if (!property.isExpanded)
            return;
        position.y += _itemHeight;
        position.height -= _itemHeight;

        EditorHelper.DrawBox(position);
        position.x += 1;
        position.y += 1;
        position.width -= 2;
        position.height -= 2;
        
        List<string> openedElements = dashboard.openedVariables;
        dashboard.scrollPosition = VariableTreeDrawer.DrawVariables(
            variables, 
            openedElements, 
            dashboard.scrollPosition,
            position, 
            VariableTree.DrawingType.MonoBehaviour);
    }

    static void SetupHeight()
    {
        
        _itemHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    bool TryGetVariables(SerializedProperty property, out IEnumerable<Variable> variables)
    {
        SerializedObject serializedObject = property.serializedObject;
        if (!(serializedObject.targetObject is MonoBehaviour monoBehaviour))
        {
            variables = null;
            return false;
        }

        variables = monoBehaviour.GetComponentsInChildren<Variable>().Where(childVariable => 
            childVariable.visibility == Variable.Visibility.Global ||
            childVariable.visibility == Variable.Visibility.Parent ||
            childVariable.gameObject == monoBehaviour.gameObject);
        return true;
    } 


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return base.GetPropertyHeight(property, label);

        if (!TryGetVariables(property, out IEnumerable<Variable> variables))
            return 0;
        
        var dashboard = (InnerVariablesDashboard)property.GetObjectOfProperty();
        var tree = new VariableTree(variables);
        List<string> openedElements = dashboard.openedVariables;
        float treeHeight = VariableTreeDrawer.TreeHeight(tree, openedElements);
        SetupHeight();
        return Mathf.Min(dashboard.MaxItemCount * _itemHeight , treeHeight+1) + 1 + _itemHeight;
    } 
}
}