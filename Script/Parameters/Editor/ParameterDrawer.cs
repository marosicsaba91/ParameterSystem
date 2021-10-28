using System;
using System.Collections.Generic;
using System.Linq;
using MUtility; 
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StateMachineSystem
{
[CustomPropertyDrawer(typeof(Parameter), useForChildren: true)]
public class ParameterDrawer : PropertyDrawer
{
    int _idHash;



    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var parameter = (Parameter)property.GetObjectOfProperty();
        Object serializedObject = property.serializedObject.targetObject;

        const float settingWith = 20f;
        Rect fullValuePos = position;
        fullValuePos.width -= settingWith + EditorGUIUtility.standardVerticalSpacing;
        Rect labelPos = fullValuePos;
        labelPos.width = EditorHelper.LabelWidth;
        Rect valuePos = fullValuePos;
        valuePos.x = labelPos.xMax;
        valuePos.width -= labelPos.width;



        ParameterComponent component = parameter.ParameterComponent;

        if (component != null)
        {
            EditorGUI.LabelField(labelPos, label);
             
            var newComp = (ParameterComponent)
                EditorGUI.ObjectField(fullValuePos, label, component, parameter.ComponentType, allowSceneObjects: true);
            
            ParameterChanged(newComp, component, serializedObject, parameter);
            
        }
        else
        {
            Undo.RecordObject(serializedObject, "Parameter Value Changed");
            switch (parameter)
            {
                case BoolParameter boolParameter:
                    boolParameter.Value = EditorGUI.Toggle(fullValuePos, label, boolParameter.Value);
                    break;
                case FloatParameter floatParameter:
                    floatParameter.Value = EditorGUI.FloatField(fullValuePos, label, floatParameter.Value);
                    break;
                case IntParameter intParameter:
                    intParameter.Value = EditorGUI.IntField(fullValuePos, label, intParameter.Value);
                    break;
                case StringParameter stringParameter:
                    stringParameter.Value = EditorGUI.TextField(fullValuePos, label, stringParameter.Value);
                    break;
                case Vector2Parameter vector2Parameter:
                    vector2Parameter.Value = EditorGUI.Vector2Field(fullValuePos, label, vector2Parameter.Value);
                    break;
                case Vector3Parameter vector3Parameter:
                    vector3Parameter.Value = EditorGUI.Vector3Field(fullValuePos, label, vector3Parameter.Value);
                    break;
                case TriggerParameter triggerParameter:
                    EditorGUI.LabelField(labelPos, label);
                    if (GUI.Button(valuePos, "Trigger"))
                        triggerParameter.OnTriggered();
                    break;
                default:
                    EditorGUI.LabelField(labelPos, label);
                    GUI.color = EditorHelper.ErrorRedColor;
                    GUIContent errorContent = EditorGUIUtility.IconContent("Error");
                    errorContent.text = "Not Supported Type !";
                    GUI.Label(valuePos, errorContent);
                    GUI.color = Color.white;
                    break;
            }
        }

        Rect settingPos = position;
        settingPos.width = settingWith;
        settingPos.x = position.xMax - settingWith;
        valuePos.width = settingWith;

        if (_idHash == 0) _idHash = "ParameterAttributeDrawer".GetHashCode();
        int id = GUIUtility.GetControlID(_idHash, FocusType.Keyboard, position);

        if (DropdownButton(id, settingPos, GUIContent.none))
        {
            List<ParameterComponent> parameters;
            switch (parameter)
            {
                case BoolParameter _:
                    parameters = ParameterHelper.GetParametersSorted<BoolComponent>();
                    break;
                case FloatParameter _:
                    parameters = ParameterHelper.GetParametersSorted<FloatComponent>();
                    break;
                case IntParameter _:
                    parameters = ParameterHelper.GetParametersSorted<IntComponent>();
                    break;
                case StringParameter _:
                    parameters = ParameterHelper.GetParametersSorted<StringComponent>();
                    break;
                case Vector2Parameter _:
                    parameters = ParameterHelper.GetParametersSorted<Vector2Component>();
                    break;
                case Vector3Parameter _:
                    parameters = ParameterHelper.GetParametersSorted<Vector3Component>();
                    break;
                case TriggerParameter _:
                    parameters = ParameterHelper.GetParametersSorted<TriggerComponent>();
                    break;
                default:
                    parameters = new List<ParameterComponent>();
                    break;
            }


            var selectables = new string[parameters.Count +1] ;
            selectables[0] = "- VALUE -";
            for (int i = 0; i < parameters.Count; i++)
                selectables[i + 1] = parameters[i].FullPathString;

            int index = parameters.IndexOf(component) + 1;

            SearchablePopup.Show(valuePos, selectables, index, OnSelect);

            void OnSelect(int i)
            {
                ParameterComponent newComp = i > 0 ? parameters[i - 1] : null;
                ParameterChanged(newComp, component, serializedObject, parameter);
            }
        }
    }

    static void ParameterChanged(ParameterComponent newComp, ParameterComponent component, Object serializedObject,
        Parameter parameter)
    {
        if (newComp != component)
        {
            Object[] changingObjects;

            if (component == null)
                changingObjects = new[] { serializedObject, newComp };
            else if (newComp == null)
                changingObjects = new[] { serializedObject, component };
            else
                changingObjects = new[] { serializedObject, component, newComp };
            Undo.RecordObjects(changingObjects, "Parameter Changed");
            parameter.ParameterComponent = newComp;
        }
    }

    public static bool DropdownButton(int id, Rect position, GUIContent content)
    {
        Event current = Event.current;
        switch (current.type)
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && current.button == 0)
                {
                    Event.current.Use();
                    return true;
                }

                break;
            case EventType.KeyDown:
                if (GUIUtility.keyboardControl == id && current.character == '\n')
                {
                    Event.current.Use();
                    return true;
                }

                break;
            case EventType.Repaint:
                EditorStyles.popup.Draw(position, content, id, false);
                break;
        }

        return false;
    } 
}
}