#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using MUtility;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

namespace PlayBox
{
public class VariableWindow : EditorWindow
{
    static float _lineH;
    static float _space;
    static int _lineCount = 0;

    [SerializeField] List<string> openedElements = new List<string>();

    [SerializeField] Vector2 scrollPosition;
    const string editorPrefsKey = "Variable Window Info";
    const string windowName = "Variable Dashboard";
    const string menuItemName = "Tools/" + windowName;

    static GUIStyle _labelStyle;
    public static GUIStyle LabelStyle => _labelStyle ??
        (_labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, alignment = TextAnchor.MiddleLeft });

    public void OnEnable()
    {
        Undo.undoRedoPerformed += Repaint;
        wantsMouseMove = true;
        string data = EditorPrefs.GetString(
            editorPrefsKey, JsonUtility.ToJson(this, prettyPrint: false));
        JsonUtility.FromJsonOverwrite(data, this); 
    }

    public void OnDisable()
    {
        Undo.undoRedoPerformed -= Repaint;
        string data = JsonUtility.ToJson(this, prettyPrint: false);
        EditorPrefs.SetString(editorPrefsKey, data);
    }

    [MenuItem(menuItemName)]
    static void ShowWindow()
    {
        var window = GetWindow<VariableWindow>();
        window.titleContent = new GUIContent(windowName);
        window.Show();
        window.openedElements = new List<string>();
    }


    void OnGUI()
    {
        _lineH = EditorGUIUtility.singleLineHeight;
        _space = EditorGUIUtility.standardVerticalSpacing;
        VariableTree variableTree = VariableHelper.GetVariableTree();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        _lineCount = 0;
        EditorGUI.indentLevel = -1;
        DrawTree(variableTree, openedElements);

        EditorGUILayout.EndScrollView();
    }


    static void DrawTree(VariableTree tree, List<string> openedElements)
    { 
        List<string> categories = tree.Path().Reverse().ToList();
        bool emptyCategory = categories.Count == 1 && categories.First() == string.Empty;
        string categoryText = emptyCategory ? string.Empty : string.Join(" / ", categories);
        var open = true;
        var pos = new Rect();
        var openable = false;
        if (!emptyCategory)
        {
            open = openedElements.Contains(categoryText);
            openable = !tree.children.IsNullOrEmpty() || tree.variables.Count > 1;
            pos = EditorGUILayout.GetControlRect(true, _lineH);

            if (openable)
            {
                bool isSelected = tree.variables.Count == 1 && Selection.Contains(tree.variables[0].gameObject);
                DrawRowColor(pos, isSelected);
                bool newOpen = EditorGUI.Foldout(pos, open, GUIContent.none);
                EditorGUI.indentLevel++;
                EditorGUI.LabelField(pos,  categories.Last());
                EditorGUI.indentLevel--;
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
        }


        if (!emptyCategory)
        {
            if (tree.variables.Count == 1)
            {
                GUIContent content =
                    openable ? GUIContent.none : new GUIContent(tree.variables[0].Path.LastOrDefault());
                DrawVariable(pos, tree.variables[0], content);
            }

            else if (open)
            {
                EditorGUI.indentLevel++;
                foreach (Variable variable in tree.variables)
                {
                    Rect variablePos = EditorGUILayout.GetControlRect(hasLabel: true, _lineH);
                    GUIContent content = EditorGUIUtility.IconContent("GameObject Icon");
                    content.text = variable.gameObject.name;
                    DrawVariable(variablePos, variable, content, isGameObject: true);
                }

                EditorGUI.indentLevel--;
            }
        }

        if (!open)
        {
            EditorGUI.indentLevel--;
            return;
        }


        foreach (KeyValuePair<string, VariableTree> child in tree.children)
        {
            EditorGUI.indentLevel++;
            DrawTree(child.Value, openedElements);
        }

        EditorGUI.indentLevel--;
    }

    const float invisibleLabelWidth = 25;

    static void DrawVariable(Rect position, Variable variable, GUIContent name, bool isGameObject = false)
    { 
        EditorGUI.indentLevel++;
        float valueWidth =  2f * position.width / 5f;
        EditorGUIUtility.labelWidth = invisibleLabelWidth;  
 
        Object obj = variable.gameObject;
        
        bool selected = Selection.Contains(obj);
        if (name != GUIContent.none)
        {
            DrawRowColor(position, selected);  
            if (isGameObject)
            {
                GUI.color = new Color(1, 1, 1, 0.75f);
                EditorGUI.LabelField(position, name, LabelStyle);
                GUI.color = Color.white;
            }
            else
                EditorGUI.LabelField(position, name);
        }

        DrawVariableValue(position, valueWidth, variable);
        
        GUI.enabled = true;
        position.width -= valueWidth;
  
        position.width -= invisibleLabelWidth;
        if (GUI.Button(position, "", LabelStyle))
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
        EditorGUI.indentLevel--;
    }

    static void DrawRowColor(Rect position, bool selected)
    {
        var selectionPos = new Rect(
            position.x - _space, position.y - (_space / 2),
            position.width + (2 * _space), position.height + (1 * _space));
         
        Color lineColor =
            selected ? GUI.skin.settings.selectionColor :
            _lineCount % 2 == 0 ? EditorHelper.tableEvenLineColor : EditorHelper.tableOddLineColor;
        EditorHelper.DrawBox(selectionPos, lineColor);
        _lineCount++;
    }

    static void DrawVariableValue(Rect position, float valueWidth, Variable variable)
    {
        var fullValuePos = new Rect(
            position.xMax - (invisibleLabelWidth + valueWidth + 2),
            position.y,
            invisibleLabelWidth + valueWidth,
            position.height);

        var valuePos = new Rect(position.xMax - valueWidth, position.y, valueWidth, position.height); 
        var recordText = $"Variable Value Changed: {variable.PathString} / {variable.name}";

        EditorGUI.BeginChangeCheck();
        Undo.RecordObjects(variable.ChangingObjects.ToArray(), recordText);

        GUI.enabled = variable.isGUISettingEnabled;
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        var serializedObject = new SerializedObject(variable);
        var valueProperty = serializedObject.FindProperty("value");

        if (valueProperty != null)
        { 
            // Value Type
            if( variable is FloatVariable || variable is IntVariable)  
                EditorGUI.PropertyField(fullValuePos,  valueProperty, new GUIContent(" "));
            else 
                EditorGUI.PropertyField(valuePos, valueProperty, GUIContent.none);
             
            PropertyInfo valuePropertyInfo = variable.GetType().GetProperty("Value");
            valuePropertyInfo.SetValue(variable, valueProperty.GetPropertyValue());

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

            focusedWindow.Repaint();
        }
        
        EditorGUI.indentLevel = indent;
    }
}

} 
#endif