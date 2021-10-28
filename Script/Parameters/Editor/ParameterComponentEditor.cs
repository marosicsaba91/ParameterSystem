using System.Linq;
using MUtility;
using UnityEditor;
using UnityEngine;

namespace StateMachineSystem
{
[CustomEditor(typeof(ParameterComponent), editorForChildClasses: true)]
public class ParameterComponentEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        var component = target as ParameterComponent;

        Rect fullRect = EditorGUILayout.GetControlRect(hasLabel: true, height: 16f, EditorStyles.layerMaskField);
        fullRect.height += 2;
        Rect labelRect = fullRect;
        labelRect.width = EditorHelper.LabelWidth;
        Rect valueRect = fullRect;
        valueRect.width = fullRect.width-labelRect.width;
        valueRect.x = labelRect.xMax;
        labelRect.width -= 4;

        Undo.RecordObjects(component.ChangingObjects.ToArray(), "Component Value Changed");

        component.PathString = EditorGUI.TextField(labelRect, component.PathString);
         
        GUI.enabled = component.isSettingEnabled;
        switch (component)
        {
            case BoolComponent boolComponent:
                boolComponent.Value = EditorGUI.Toggle(valueRect, boolComponent.Value);
                break;
            case FloatComponent floatComponent:
                floatComponent.Value = EditorGUI.FloatField(valueRect, floatComponent.Value);
                break;
            case IntComponent intComponent:
                intComponent.Value = EditorGUI.IntField(valueRect, intComponent.Value);
                break;
            case StringComponent stringComponent:
                stringComponent.Value = EditorGUI.TextField(valueRect, stringComponent.Value);
                break;
            case Vector2Component vector2Component: 
                vector2Component.Value = EditorGUI.Vector2Field(valueRect, GUIContent.none, vector2Component.Value);
                break;
            case Vector3Component vector3Component: 
                vector3Component.Value = EditorGUI.Vector3Field(valueRect, GUIContent.none, vector3Component.Value);
                break;
            case TriggerComponent triggerComponent: 
                if (GUI.Button(valueRect, "Trigger"))
                    triggerComponent.OnTriggered();
                break;
            default:
                GUI.color = EditorHelper.ErrorRedColor;
                GUIContent errorContent = EditorGUIUtility.IconContent("Error");
                errorContent.text = "Not Supported Type !";
                GUI.Label(valueRect, errorContent);
                GUI.color = Color.white;
                break;
        }
        GUI.enabled = true;

    }
}
}