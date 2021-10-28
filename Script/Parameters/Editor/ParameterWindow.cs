#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using MUtility;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace PlayBox
{
public class ParameterWindow : EditorWindow
{
    static float _lineH;
    static float _space;
    static int _lineCount = 0;

    [SerializeField] List<string> openedElements = new List<string>();

    [SerializeField] Vector2 scrollPosition;
    const string editorPrefsKey = "Parameter Window Info";

    static GUIStyle _labelStyle;
    public static GUIStyle LabelStyle =>
        _labelStyle = _labelStyle ?? new GUIStyle(GUI.skin.label){fontSize = 10, alignment = TextAnchor.MiddleLeft};   
    
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

    [MenuItem("Tools/Parameter Dashboard")]
    static void ShowWindow()
    {
        var window = GetWindow<ParameterWindow>();
        window.titleContent = new GUIContent("Parameter Dashboard");
        window.Show();
        window.openedElements = new List<string>();
    }


    void OnGUI()
    {
        _lineH = EditorGUIUtility.singleLineHeight;
        _space = EditorGUIUtility.standardVerticalSpacing;
        ParameterTree parameterTree = ParameterHelper.GetSceneParameterTree();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        _lineCount = 0;
        EditorGUI.indentLevel = -1;
        DrawTree(parameterTree, openedElements);

        EditorGUILayout.EndScrollView();
    }


    static void DrawTree(ParameterTree tree, List<string> openedElements)
    { 
        List<string> categories = tree.Categories().Reverse().ToList();
        bool emptyCategory = categories.Count == 1 && categories.First() == string.Empty;
        string categoryText = emptyCategory ? string.Empty : string.Join(" / ", categories);
        var open = true;
        var pos = new Rect();
        var openable = false;
        if (!emptyCategory)
        {
            open = openedElements.Contains(categoryText);
            openable = !tree.childCategories.IsNullOrEmpty() || tree.parameters.Count > 1;
            pos = EditorGUILayout.GetControlRect(true, _lineH);

            if (openable)
            {
                bool isSelected = tree.parameters.Count == 1 && Selection.Contains(tree.parameters[0].gameObject);
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
            if (tree.parameters.Count == 1)
            {
                GUIContent content =
                    openable ? GUIContent.none : new GUIContent(tree.parameters[0].Path.LastOrDefault());
                DrawParameter(pos, tree.parameters[0], content);
            }

            else if (open)
            {
                EditorGUI.indentLevel++;
                foreach (Parameter parameter in tree.parameters)
                {
                    Rect parameterPos = EditorGUILayout.GetControlRect(hasLabel: true, _lineH);
                    GUIContent content = EditorGUIUtility.IconContent("GameObject Icon");
                    content.text = parameter.gameObject.name;
                    DrawParameter(parameterPos, parameter, content, isGameObject: true);
                }

                EditorGUI.indentLevel--;
            }
        }

        if (!open)
        {
            EditorGUI.indentLevel--;
            return;
        }


        foreach (KeyValuePair<string, ParameterTree> child in tree.childCategories)
        {
            EditorGUI.indentLevel++;
            DrawTree(child.Value, openedElements);
        }

        EditorGUI.indentLevel--;
    }

    const float invisibleLabelWidth = 25;

    static void DrawParameter(Rect position, Parameter parameter, GUIContent name, bool isGameObject = false)
    { 
        EditorGUI.indentLevel++;
        float valueWidth =  2f * position.width / 5f;
        EditorGUIUtility.labelWidth = invisibleLabelWidth;  
 
        Object obj = parameter.gameObject;
        
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

        DrawProperty(position, valueWidth, parameter);
        
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

    static void DrawProperty(Rect position, float valueWidth, Parameter parameter)
    {
        var fullValuePos = new Rect(
            position.xMax - (invisibleLabelWidth + valueWidth + 2),
            position.y,
            invisibleLabelWidth + valueWidth,
            position.height);

        var valuePos = new Rect(position.xMax - valueWidth, position.y, valueWidth, position.height); 
        var recordText = $"Parameter Value Changed: {parameter.PathString} / {parameter.name}";

        EditorGUI.BeginChangeCheck();
        Undo.RecordObjects(parameter.ChangingObjects.ToArray(), recordText);

        GUI.enabled = parameter.isSettingEnabled;
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        switch (parameter)
        {
            case BoolParameter boolComponent: 
                boolComponent.Value = EditorGUI.Toggle(valuePos, boolComponent.Value);
                break;
            case FloatParameter floatComponent: 
                floatComponent.Value = EditorGUI.FloatField(fullValuePos, " ", floatComponent.Value);
                break;
            case IntParameter intComponent: 
                intComponent.Value = EditorGUI.IntField(fullValuePos, " ", intComponent.Value);
                break;
            case StringParameter stringComponent: 
                stringComponent.Value = EditorGUI.TextField(valuePos, stringComponent.Value);
                break;
            case Vector2Parameter vector2Component:
                fullValuePos.y -= _lineH + _space; // HACK 
                vector2Component.Value = EditorGUI.Vector2Field(valuePos, GUIContent.none, vector2Component.Value);
                break;
            case Vector3Parameter vector3Component:
                fullValuePos.y -= _lineH + _space; // HACK 
                vector3Component.Value = EditorGUI.Vector3Field(valuePos, GUIContent.none, vector3Component.Value);
                break;
            case TriggerParameter triggerComponent: 
                if (GUI.Button(valuePos, "Trigger"))
                    triggerComponent.OnTriggered();
                break;
            default:
                GUI.color = EditorHelper.ErrorRedColor;
                GUIContent errorContent = EditorGUIUtility.IconContent("Error");
                errorContent.text = "Not Supported Type !";
                GUI.Label(valuePos, errorContent);
                GUI.color = Color.white;
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object dirty in parameter.ChangingObjects)
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