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
[CustomPropertyDrawer(typeof(Parameter), useForChildren: true)]
public class ParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var parameter = (Parameter)property.GetObjectOfProperty();
        Object serializedObject = property.serializedObject.targetObject;

        Rect fullValuePos = position;
        Rect labelPos = fullValuePos;
        labelPos.width = EditorHelper.LabelWidth;
        fullValuePos.x = labelPos.xMax;
        fullValuePos.width = position.width - EditorHelper.LabelWidth;
        Rect dropdownFieldPos = fullValuePos;

        EditorGUI.LabelField(labelPos, label);

        Type type = GetType(property);
        var isLocal = false;
        string defaultName = label.text;
        GameObject gameObject = null;
        
        if (serializedObject is Component component)
        {
            if (TryGetLocalAttribute(property, out LocalParameterAttribute localAttribute))
            {
                gameObject = component.gameObject;
                isLocal = true;
                if (!string.IsNullOrEmpty(localAttribute.defaultName))
                    defaultName = localAttribute.defaultName;
            }
        }
        else
        {
            GUIContent error = EditorGUIUtility.IconContent("console.warnicon");
            error.text = "Can't Reference Parameter from ScriptableObject";
            GUI.Label(dropdownFieldPos,error);
            return;
        }

        // Draw Object Field
        if (!isLocal)
        {
            float space = EditorGUIUtility.standardVerticalSpacing;
            Rect objectFieldPos = fullValuePos;
            objectFieldPos.x = labelPos.xMax;
            objectFieldPos.width = Mathf.Round((objectFieldPos.width -space) / 2f);
            dropdownFieldPos.x = objectFieldPos.xMax + space;
            dropdownFieldPos.width = fullValuePos.width - objectFieldPos.width - space;
            
            var newParam = (Parameter)
                EditorGUI.ObjectField(objectFieldPos, GUIContent.none, parameter, type, allowSceneObjects: true);
            TryChange(property, newParam, parameter, serializedObject);
            if (parameter != null)
                gameObject = parameter.gameObject;
        }
 
        // Draw Dropdown
        GUI.enabled = gameObject != null;

        List<Parameter> parameters = gameObject == null
            ? new List<Parameter>()
            : gameObject.GetComponents(type).Cast<Parameter>().ToList();
        
        int index = parameters.IndexOf(parameter) + 1; 
        bool createNewOption = isLocal && index == 0;
        
        var selectables = new string[parameters.Count + (createNewOption ? 2 : 1)];
        selectables[0] = "None";
        if (createNewOption)
            selectables[selectables.Length-1] = "[ Create New ]";

        for (var i = 0; i < parameters.Count; i++)
            selectables[i + 1] = parameters[i].PathString;

        int newIndex = EditorGUI.Popup(dropdownFieldPos, index, selectables);
        if (newIndex == index) return;
        if (newIndex <= 0)
            TryChange(property, newParam: null, parameter, serializedObject);
        else if (newIndex > parameters.Count)
        {
            var newParameter = (Parameter)gameObject.AddComponent(type);
            newParameter.PathString = defaultName;
            TryChange(property, newParameter, parameter, serializedObject);
        }else
            TryChange(property, parameters[newIndex - 1], parameter, serializedObject);
    }



    public static bool TryGetLocalAttribute(SerializedProperty prop, out LocalParameterAttribute localAttribute)
    {
        localAttribute = null;
        if (prop == null) return false; 

        Type t = prop.serializedObject.targetObject.GetType();

        FieldInfo p = null;
        foreach (string name in prop.propertyPath.Split('.'))
        {
            const BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            p = t.GetField(name, binding);
            if (p == null) return false;
            t = p.FieldType;
        }

        if (p == null) return false;

        var attributes = 
            p.GetCustomAttributes(typeof(LocalParameterAttribute), inherit: false) as LocalParameterAttribute[];

        if (attributes.Length == 0) return false;
        localAttribute = attributes[0];
        return true;
    }

    static void TryChange(SerializedProperty property, Parameter newParam, Parameter parameter,
        Object serializedObject)
    {
        if (newParam != parameter)
        {
            Undo.RecordObject(serializedObject, "Parameter Changed");
            property.objectReferenceValue = newParam;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public static Type GetType(SerializedProperty property)
    {
        Type parentType = property.serializedObject.targetObject.GetType();
        const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo fi = parentType.GetField(property.propertyPath, binding);
        return fi.FieldType;
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