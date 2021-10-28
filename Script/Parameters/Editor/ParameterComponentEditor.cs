using System.Linq;
using MUtility;
using UnityEditor;
using UnityEngine;

namespace PlayBox
{
[CustomEditor(typeof(Parameter), editorForChildClasses: true)]
public class ParameterComponentEditor : Editor
{
    public override void OnInspectorGUI()
    { 
        var component = target as Parameter;

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
            case BoolParameter boolComponent:
                boolComponent.Value = EditorGUI.Toggle(valueRect, boolComponent.Value);
                break;
            case FloatParameter floatComponent:
                floatComponent.Value = EditorGUI.FloatField(valueRect, floatComponent.Value);
                break;
            case IntParameter intComponent:
                intComponent.Value = EditorGUI.IntField(valueRect, intComponent.Value);
                break;
            case StringParameter stringComponent:
                stringComponent.Value = EditorGUI.TextField(valueRect, stringComponent.Value);
                break;
            case Vector2Parameter vector2Component: 
                vector2Component.Value = EditorGUI.Vector2Field(valueRect, GUIContent.none, vector2Component.Value);
                break;
            case Vector3Parameter vector3Component: 
                vector3Component.Value = EditorGUI.Vector3Field(valueRect, GUIContent.none, vector3Component.Value);
                break;
            case TriggerParameter triggerComponent: 
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