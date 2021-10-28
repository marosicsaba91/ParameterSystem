#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using MUtility;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace StateMachineSystem
{
public class ParameterWindow : EditorWindow
{
    static float _lineH;
    static float _space;
    static int _lineCount = 0;

    [SerializeField] List<string> openedElements = new List<string>();

    [SerializeField] Vector2 scrollPosition;
    const string editorPrefsKey = "Parameter Window Info";

    static GUIStyle _boldFoldoutStyle;
    public static GUIStyle BoldFoldoutStyle => 
        _boldFoldoutStyle = _boldFoldoutStyle ?? new GUIStyle(EditorStyles.foldout)
            {fontStyle = FontStyle.Bold};
    
    static GUIStyle _labelStyle;
    public static GUIStyle LabelStyle =>
        _labelStyle = _labelStyle ?? new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleLeft};
    
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
        DrawTree(parameterTree, openedElements);

        EditorGUILayout.EndScrollView();
    }


    static void DrawTree(ParameterTree tree, List<string> openedElements)
    {
        List<string> categories = tree.Categories().Reverse().ToList();
        bool emptyCategory = categories.Count == 1 && categories.First() == string.Empty;
        if (!emptyCategory)
            categories.RemoveAt(0);
        string categoryText = emptyCategory ? string.Empty : string.Join(" / ", categories); 
        var open = true;
        if (!emptyCategory || tree.parameters.Any())
        {
            open = emptyCategory || openedElements.Contains(categoryText);
            if (!emptyCategory)
            {
                Rect pos = EditorGUILayout.GetControlRect(true, _lineH);
                DrawLineColor(pos, false);
                bool newOpen = EditorGUI.Foldout(pos, open, categories.Last(), BoldFoldoutStyle);
                if (open != newOpen)
                {
                    open = newOpen;
                    if (open)
                        openedElements.Add(categoryText);
                    else
                        openedElements.Remove(categoryText);
                }
            }
        }

        if (!open) return;
        if (!emptyCategory)
            EditorGUI.indentLevel++;
        foreach (ParameterComponent parameter in tree.parameters)
        {
            Rect parameterPos = EditorGUILayout.GetControlRect(hasLabel: true, _lineH);
            DrawParameter(parameterPos, parameter);
        }

        foreach (KeyValuePair<string, ParameterTree> child in tree.childCategories)
            DrawTree(child.Value, openedElements);
        if (!emptyCategory)
            EditorGUI.indentLevel--;
    }
    const float invisibleLabelWidth = 25;

    static void DrawParameter(Rect position, ParameterComponent parameterComponent)
    { 
        float valueWidth =  2f * position.width / 5f;
        EditorGUIUtility.labelWidth = invisibleLabelWidth;  
 
        Object obj = parameterComponent.gameObject;
        
        GUIContent content = EditorGUIUtility.IconContent("GameObject Icon");
        content.text = obj.name; 
        content.tooltip = "Parameter as Component";
        
        bool selected = Selection.Contains(obj);
        DrawLineColor(position, selected);

        DrawProperty(position, valueWidth, parameterComponent, content);
        
        GUI.enabled = true;
        position.width -= valueWidth;
 
        
        EditorGUI.LabelField(position, content);
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
    }

    static void DrawLineColor(Rect position, bool selected)
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

    static void DrawProperty(Rect position, float valueWidth, ParameterComponent parameterComponent, GUIContent content)
    {
        var fullValuePos = new Rect(
            position.xMax - (invisibleLabelWidth + valueWidth),
            position.y,
            invisibleLabelWidth + valueWidth,
            position.height);

        var valuePos = new Rect(position.xMax - valueWidth, position.y, valueWidth, position.height); 
        var recordText = $"Parameter Value Changed: {parameterComponent.PathString} / {parameterComponent.name}";

        EditorGUI.BeginChangeCheck();
        Undo.RecordObjects(parameterComponent.ChangingObjects.ToArray(), recordText);

        GUI.enabled = parameterComponent.isSettingEnabled;
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        
        switch (parameterComponent)
        {
            case BoolComponent boolComponent:
                content.tooltip = $"Bool {content.tooltip}"; 
                boolComponent.Value = EditorGUI.Toggle(valuePos, boolComponent.Value);
                break;
            case FloatComponent floatComponent:
                content.tooltip = $"Float {content.tooltip}";
                floatComponent.Value = EditorGUI.FloatField(fullValuePos, " ", floatComponent.Value);
                break;
            case IntComponent intComponent:
                content.tooltip = $"Int {content.tooltip}";
                intComponent.Value = EditorGUI.IntField(fullValuePos, " ", intComponent.Value);
                break;
            case StringComponent stringComponent:
                content.tooltip = $"Text {content.tooltip}";
                stringComponent.Value = EditorGUI.TextField(valuePos, stringComponent.Value);
                break;
            case Vector2Component vector2Component:
                fullValuePos.y -= _lineH + _space; // HACK
                content.tooltip = $"Vector2 {content.tooltip}";
                vector2Component.Value = EditorGUI.Vector2Field(valuePos, GUIContent.none, vector2Component.Value);
                break;
            case Vector3Component vector3Component:
                fullValuePos.y -= _lineH + _space; // HACK
                content.tooltip = $"Vector3 {content.tooltip}";
                vector3Component.Value = EditorGUI.Vector3Field(valuePos, GUIContent.none, vector3Component.Value);
                break;
            case TriggerComponent triggerComponent:
                content.tooltip = $"Trigger {content.tooltip}";
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
            foreach (Object dirty in parameterComponent.ChangingObjects)
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