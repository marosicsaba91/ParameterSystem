using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MUtility;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayBox
{
static class VariableTreeDrawer
{
    static float _lineH;
    static float _space;
    static float _indentW;
    static int _lineCount = 0;
    const float invisibleLabelWidth = 25;
    
    static GUIStyle _labelStyle;
    static GUIStyle LabelStyle =>
        _labelStyle ?? (_labelStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 10, alignment = TextAnchor.MiddleLeft });

    static VariableTree.DrawingType _drawingType;
    
     public static Vector2 DrawVariables(
         IEnumerable<Variable> variables,
         List<string> openedElements,
         Vector2 scrollPosition,
         Rect position,
         VariableTree.DrawingType drawingType = VariableTree.DrawingType.Window)
    {
        _lineH = EditorGUIUtility.singleLineHeight;
        _space = EditorGUIUtility.standardVerticalSpacing;
        _indentW = EditorHelper.indentWidth;
        _drawingType = drawingType;
        var variableTree = new VariableTree(variables);
        float treeHeight = TreeHeight(variableTree, openedElements);

        const float scrollbarSize = 14;
        float width = position.width;
        
        var viewRect = new Rect(position.position, new Vector2(position.width, treeHeight));
        if (treeHeight > position.height)
        {
            viewRect.width -= scrollbarSize;
            width -= scrollbarSize;
        }
        
        scrollPosition = GUI.BeginScrollView(position, scrollPosition, viewRect);
        

        _lineCount = 0;        
        Vector2 pos = position.position;

        DrawTree(
            variableTree,
            openedElements,
            pos.x,
            pos.y,
            width,
            pos.x,
            width,
            drawingType);

        GUI.EndScrollView();
        return scrollPosition;
    }

     internal static float TreeHeight(VariableTree tree, List<string> openedElements)
    {
        float height = 0;
        bool isRoot = tree.IsRoot;
        string categoryText = tree.CategoryText;
        var open = true;
        if (!isRoot)
        { 
            open = openedElements.Contains(categoryText);  
            if (tree.variables.Count == 1)
                height += _lineH + _space; 
            
            else if (open)
            {
                foreach (Variable variable in tree.variables) 
                    height += VariableHeight(variable) + _space; 
                height += _lineH + _space;
            }
        }

        if (!open)
            return _lineH;

        foreach (KeyValuePair<string, VariableTree> child in tree.children)
            height += TreeHeight(child.Value, openedElements) + _space;
        return height - _space;
    }

    static void DrawTree(
        VariableTree tree, 
        List<string> openedElements,
        float xIndented,
        float y,
        float indentedWidth,
        float xBase,
        float fullWidth,
        VariableTree.DrawingType drawingType)
    {
        bool isRoot = tree.IsRoot;
        string categoryText = tree.CategoryText;
        var open = true;

        //var h = TreeHeight(tree, openedElements);
        //var testPos = new Rect(position.x, position.y, width, h);
        //EditorHelper.DrawBox(testPos, new Color(1,1,0, 0.15f));

        if (!isRoot)
        {
            open = openedElements.Contains(categoryText);
            bool openable = !tree.children.IsNullOrEmpty() || tree.variables.Count > 1;

            if (openable)
            {
                bool isSelected = tree.variables.Count == 1 && Selection.Contains(tree.variables[0].GameObject);
                var fullPos = new Rect(xBase, y, fullWidth, _lineH);
                var intendedPos = new Rect(xIndented, y, indentedWidth, _lineH);
                DrawRowColor(fullPos, isSelected);
                
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = _drawingType == VariableTree.DrawingType.Window ? -1 : 0;
                bool newOpen = EditorGUI.Foldout(intendedPos, open, GUIContent.none);
                EditorGUI.indentLevel = indent;

                GUI.Label(intendedPos, tree.Path().First()); 
                if (open != newOpen)
                {
                    open = newOpen;
                    if (open)
                        openedElements.Add(categoryText);
                    else
                        openedElements.Remove(categoryText);
                }
            }
            else
                open = true;

            if (tree.variables.Count == 1)
            { 
                GUIContent content =
                    openable ? GUIContent.none : new GUIContent(tree.variables[0].Path.LastOrDefault());
                DrawVariable( xIndented, y, indentedWidth, xBase, fullWidth, tree.variables[0], content, isGameObject:false, drawingType);
                y += _lineH + _space;
            }
            else if (open)
            {
                y += _lineH + _space;
                xIndented += _indentW;
                foreach (Variable variable in tree.variables)
                {
                    GUIContent content = EditorGUIUtility.IconContent("GameObject Icon");
                    content.text = variable.GameObject.name;
                    float variableHeight = VariableHeight(variable);

                    DrawVariable( xIndented, y, indentedWidth, xBase, fullWidth, variable, content, isGameObject: true, drawingType);

                    y += variableHeight + _space;
                } 
                xIndented -= _indentW;
            }
        }

        if (!open)
            return;
  
        xIndented += _indentW;
        indentedWidth -= _indentW;
        foreach (KeyValuePair<string, VariableTree> child in tree.children)
        {
            float treeHeight = TreeHeight(child.Value, openedElements);
            DrawTree(child.Value, openedElements, xIndented, y, indentedWidth, xBase, fullWidth, drawingType);
            y += treeHeight + _space;
        }
    }

    static void DrawVariable(
        float xIndented,
        float y,
        float indentedWidth,
        float xBase,
        float fullWidth,
        Variable variable, 
        GUIContent name,
        bool isGameObject, 
        VariableTree.DrawingType drawingType)
    {
        float valueWidth = fullWidth * 2f / 5f;
        EditorGUIUtility.labelWidth = invisibleLabelWidth;

        Object obj = variable.GameObject;

        bool selected = drawingType == VariableTree.DrawingType.Window && Selection.Contains(obj); 
        float height = VariableHeight(variable);
        var fullRowPos = new Rect(xBase, y ,fullWidth, height);
        var fullValuePos = new Rect(xIndented, y ,indentedWidth, height);
        if (name != GUIContent.none)
        {
            DrawRowColor(fullRowPos, selected);
            if (isGameObject)
            {
                GUI.color = new Color(1, 1, 1, 0.75f);
                GUI.Label(fullValuePos, name, LabelStyle);
                GUI.color = Color.white;
            }
            else
                GUI.Label(fullValuePos, name);
        }

        DrawVariableValue(fullRowPos, valueWidth, variable);

        GUI.enabled = true;
        Rect labelPos = fullValuePos;
        labelPos.width -= valueWidth;
        labelPos.width -= invisibleLabelWidth;
        if (GUI.Button(labelPos, "", LabelStyle))
        {
            if (drawingType == VariableTree.DrawingType.Window)
            {
                bool ctrl = Event.current.control;
                if (ctrl)
                {
                    List<Object> objects = Selection.objects.ToList();
                    if (selected && Selection.objects.Length == 1)
                        objects.Remove(obj);
                    else
                        objects.Add(obj);
                    Selection.objects = objects.ToArray();
                }
                else
                    Selection.objects = selected ? Array.Empty<Object>() : new[] { obj };
            }
            else if (drawingType == VariableTree.DrawingType.MonoBehaviour)
                EditorGUIUtility.PingObject(obj);
        }
    }

    static float VariableHeight(Variable variable) => _lineH;

    static void DrawRowColor(Rect position, bool selected)
    {
        Color lineColor =
            selected ? GUI.skin.settings.selectionColor :
            _lineCount % 2 == 0 ? EditorHelper.tableEvenLineColor : EditorHelper.tableOddLineColor;
        position.y -= _space / 2f;
        position.height += _space;
        EditorHelper.DrawBox(position, lineColor);
        _lineCount++;
    }

    static void DrawVariableValue(Rect position, float valueWidth, Variable variable)
    {
        var numberValuePos = new Rect(
            position.xMax - (valueWidth + invisibleLabelWidth + 2),
            position.y,
            invisibleLabelWidth + valueWidth + 2,
            position.height);
        var valuePos = new Rect(position.xMax - valueWidth, position.y, valueWidth, position.height); 

        var recordText = $"Variable Value Changed: {variable.NiceName}";

        EditorGUI.BeginChangeCheck();
        Undo.RecordObjects(variable.ChangingObjects.ToArray(), recordText);

        GUI.enabled = variable.isGUISettingEnabled; 
        
        var serializedObject = new SerializedObject(variable.sourceComponent); 
        var valueProperty = serializedObject.FindProperty(variable.ElementName);
        
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        if (valueProperty != null)
        { 
            // Value Type
            EditorGUIUtility.labelWidth = invisibleLabelWidth;
            if( variable is FloatVariable || variable is IntVariable)  
                EditorGUI.PropertyField(numberValuePos,  valueProperty, new GUIContent(" "));
            else 
                EditorGUI.PropertyField(valuePos, valueProperty, GUIContent.none);
             
            PropertyInfo valuePropertyInfo = variable.GetType().GetProperty("Value");
            valuePropertyInfo.SetValue(variable, valueProperty.GetObjectOfProperty());

            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            // Event Type
            if (variable is EventVariable eventVariable)
            {
                MethodInfo invokeMethod = eventVariable.GetType().GetMethod("InvokeEvent");
                if (invokeMethod != null)
                {
                    if (GUI.Button(valuePos, eventVariable.ToString()))
                        invokeMethod.Invoke(variable, new object[0]);
                }
            }
            else
            {
                // Not Supported Type!
                GUI.color = EditorHelper.ErrorRedColor;
                GUIContent errorContent = EditorGUIUtility.IconContent("Error");
                errorContent.text = "Not Supported Type!";
                GUI.Label(valuePos, errorContent);
                GUI.color = Color.white;
            }
        } 
        
        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object dirty in variable.ChangingObjects)
            {
                if (!(dirty is ScriptableObject)) continue; 
                EditorUtility.SetDirty(dirty);
            }
            // focusedWindow.Repaint();
        } 
        EditorGUI.indentLevel = indent;
    }
}
}