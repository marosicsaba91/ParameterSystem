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
[CustomPropertyDrawer(typeof(Variable), useForChildren: true)]
public class VariableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var variable = (Variable)property.GetObjectOfProperty();
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
            if (TryGetLocalAttribute(property, out LocalVariableAttribute localAttribute))
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
            error.text = "Can't Reference Variable from ScriptableObject";
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
            
            var newParam = (Variable)
                EditorGUI.ObjectField(objectFieldPos, GUIContent.none, variable, type, allowSceneObjects: true);
            TryChange(property, newParam, variable, serializedObject);
            if (variable != null)
                gameObject = variable.gameObject;
        }
 
        // Draw Dropdown
        GUI.enabled = gameObject != null;

        List<Variable> variables = gameObject == null
            ? new List<Variable>()
            : gameObject.GetComponents(type).Cast<Variable>().ToList();
        
        int index = variables.IndexOf(variable) + 1; 
        bool createNewOption = isLocal && index == 0;
        
        var selectables = new string[variables.Count + (createNewOption ? 2 : 1)];
        selectables[0] = "None";
        if (createNewOption)
            selectables[selectables.Length-1] = "[ Create New ]";

        for (var i = 0; i < variables.Count; i++)
            selectables[i + 1] = variables[i].PathString;

        int newIndex = EditorGUI.Popup(dropdownFieldPos, index, selectables);
        if (newIndex == index) return;
        if (newIndex <= 0)
            TryChange(property, newParam: null, variable, serializedObject);
        else if (newIndex > variables.Count)
        {
            var newVariable = (Variable)gameObject.AddComponent(type);
            newVariable.PathString = defaultName;
            TryChange(property, newVariable, variable, serializedObject);
        }else
            TryChange(property, variables[newIndex - 1], variable, serializedObject);
    }



    public static bool TryGetLocalAttribute(SerializedProperty prop, out LocalVariableAttribute localAttribute)
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
            p.GetCustomAttributes(typeof(LocalVariableAttribute), inherit: false) as LocalVariableAttribute[];

        if (attributes.Length == 0) return false;
        localAttribute = attributes[0];
        return true;
    }

    static void TryChange(SerializedProperty property, Variable newParam, Variable variable,
        Object serializedObject)
    {
        if (newParam != variable)
        {
            Undo.RecordObject(serializedObject, "Variable Changed");
            property.objectReferenceValue = newParam;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    /*
    public static Type GetType(SerializedProperty property)
    {
        Type parentType = property.serializedObject.targetObject.GetType();
        const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo fi = parentType.GetField(property.propertyPath, binding);
        return fi.FieldType;
    }
    */
    
    static Type GetType(SerializedProperty property)
    {
        const BindingFlags binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        string[] path = property.propertyPath.Split('.');
        Type fieldType = property.serializedObject.GetTargetType();
        for (var i = 0; i < path.Length; i++)
            fieldType = fieldType.GetField(path[i], binding).FieldType;
 
        return fieldType;  
    }
    
}
}