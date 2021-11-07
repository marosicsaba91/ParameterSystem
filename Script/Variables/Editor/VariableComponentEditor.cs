using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MUtility;
using UnityEditor; 
using UnityEngine;

namespace PlayBox
{
[CustomEditor(typeof(Variable), editorForChildClasses: true)]
public class VariableComponentEditor : Editor
{
    SerializedProperty _valueProperty;
    MethodInfo _eventInvokeMethod;
    PropertyInfo _valuePropertyInfo;
    bool _isOpen;

    static GUIStyle _popupStyle = null;
    static GUIStyle PopupStyle => _popupStyle = _popupStyle ?? new GUIStyle(EditorStyles.miniButton) { fontSize = 10 };
    
    void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        _valueProperty = serializedObject.FindProperty("value");
        var component = target as Variable;
        if (component is EventVariable eventVariable)
            _eventInvokeMethod = eventVariable.GetType().GetMethod("InvokeEvent"); 
        _valuePropertyInfo = component.GetType().GetProperty("Value");  
    }
    
    public override void OnInspectorGUI()
    { 
        var variable = target as Variable;
        float space = EditorGUIUtility.standardVerticalSpacing;
        const float toggleWidth = 16;

        Rect fullRect = EditorGUILayout.GetControlRect(hasLabel: true, height: 16f, EditorStyles.layerMaskField);
        fullRect.height += 2;
        
        Rect showToggleRect = fullRect;
        showToggleRect.width = toggleWidth;
        showToggleRect.x = space * 2;
        
        Rect labelRect = fullRect;
        labelRect.x = showToggleRect.xMax + space;
        labelRect.width = EditorHelper.LabelWidth - space * 2;
        
        Rect valueRect = fullRect;
        valueRect.width = fullRect.width - EditorHelper.LabelWidth;
        valueRect.x = labelRect.xMax;
        labelRect.width -= 4;
        

        Undo.RecordObjects(variable.ChangingObjects.ToArray(), "Component Value Changed");

        // Show On Dashboard
        string path = variable.PathString;
        bool hasPath = !string.IsNullOrEmpty(path);
        GUI.enabled = hasPath;
        bool show =
            EditorGUI.Toggle(showToggleRect, variable.showOnDashboard && hasPath);
        if (hasPath)
            variable.showOnDashboard = show;
        GUI.enabled = true ;
        var tooltipContent = new GUIContent("", null, "Show on Dashboard");
        GUI.Label(showToggleRect, tooltipContent);
        
        // Label
        variable.PathString = EditorGUI.TextField(labelRect, path);
        
        // Foldout
        bool hasFunctionText = !string.IsNullOrEmpty(variable.FunctionUniqName);
        bool hasAvailableFunctions = variable.AvailableFunctions != null && variable.AvailableFunctions.Any(); 
        if (hasFunctionText || hasAvailableFunctions)
        {
            Rect popupRect = fullRect;
            MethodInfo function = variable.SourceFunction;
            string text;
            if (function!= null)
            {
                var attribute = function.GetCustomAttribute<PlayBoxFunctionAttribute>();
                text = attribute != null ? attribute.ShortName(function) : variable.FunctionUniqName;
            }
            else if (hasFunctionText)
                text = variable.FunctionUniqName;
            else
                text = "Value"; 
            var popupContent = new GUIContent(text);
            if(!variable.HasValidSource)   
                GUI.color = EditorHelper.ErrorRedColor;
            
            float w = PopupStyle.CalcSize(popupContent).x + 2;
            popupRect.width = w;
            popupRect.x = valueRect.xMax - w;
            if (GUI.Button(popupRect, popupContent, PopupStyle))
                _isOpen = !_isOpen;
            valueRect.width -= space + w;
        }
        
        GUI.color = Color.white;
        // Value
        GUI.enabled = variable.isGUISettingEnabled;
        if (_valueProperty != null)
        {
            EditorGUI.PropertyField(valueRect, _valueProperty, GUIContent.none);
            _valuePropertyInfo.SetValue(variable, _valueProperty.GetPropertyValue());
        }
        else if (_eventInvokeMethod != null)
        { 
            if (GUI.Button(valueRect,  ((EventVariable) variable).ToString()))
                _eventInvokeMethod.Invoke(variable, new object[0]);
        }
        else
        {
            GUI.color = EditorHelper.ErrorRedColor;
            GUIContent errorContent = EditorGUIUtility.IconContent("Error");
            errorContent.text = "Not Supported Type!";
            GUI.Label(valueRect, errorContent);
            GUI.color = Color.white;
        }

        if (_isOpen)
            DrawFunction(variable);

        GUI.enabled = true;
    }
    

    const int guiOffsetX = 6;
    void DrawFunction(Variable variable)
    {
        EditorGUILayout.Space(2); 
        Rect foldoutRect = EditorGUILayout.GetControlRect(hasLabel: true, height: 16f, EditorStyles.layerMaskField);
        foldoutRect.x += guiOffsetX;
        foldoutRect.width -= guiOffsetX;
        var functionDisplayNames = new List<string> { "VALUE" };
        var index = 0;
        var extraValues = 1;
        if (!variable.HasValidSource)
        {
            functionDisplayNames.Add(variable.FunctionUniqName);
            index = 1;
            extraValues++;
        }

        int i = extraValues;
        if (variable.AvailableFunctions != null)
            foreach (KeyValuePair<string, MethodInfo> function in variable.AvailableFunctions)
            {
                var attribute = function.Value.GetCustomAttribute<PlayBoxFunctionAttribute>();
                if (attribute == null) continue;
                functionDisplayNames.Add(attribute.DisplayName(function.Value));
                
                if (variable.FunctionUniqName == attribute.UniqName(function.Value))
                    index = i;
                i++;
            }
        
        EditorGUI.LabelField(foldoutRect, "Source Function");
        foldoutRect.x += EditorHelper.LabelWidth - guiOffsetX;
        foldoutRect.width -= EditorHelper.LabelWidth - guiOffsetX; 
        
        
        if(!variable.HasValidSource)   
            GUI.color = EditorHelper.ErrorRedColor;
        int newIndex = EditorGUI.Popup(foldoutRect,index, functionDisplayNames.ToArray());

        if (newIndex != index)
        {
            if (newIndex > extraValues - 1)
            {
                MethodInfo[] functions = variable.AvailableFunctions.Values.ToArray();
                variable.SourceFunction = functions[newIndex - extraValues]; 
            }
            else if(newIndex == 0)
                variable.SourceFunction = null;
        }
        
        DrawFunctionParameters(variable, variable.SourceFunction);
    }
 
    // TODO
    
    void DrawFunctionParameters(Variable variable, MethodInfo methodInfo)
    {
        if(methodInfo == null) return;
        for (var index = 0; index < methodInfo.GetParameters().Length; index++)
        { 
            ParameterInfo parameter = methodInfo.GetParameters()[index];
            DrawFunctionParameter(variable, index, parameter);
        }
    }

    void DrawFunctionParameter(Variable variable, int index, ParameterInfo parameter)
    { 
        Type parameterType = parameter.ParameterType;
        bool isArray = parameterType.IsArray;
        if (isArray)
            parameterType = parameterType.GetElementType();
        Parameter parameterValue = variable.GetParameterAt(index);
        Debug.Log(variable.GetType());
        var valueVariable = (ValueVariable<object>) variable;
        if (isArray)
        {
            if (parameterValue == null) 
                parameterValue = valueVariable.CreateNewParameter();
            var parameterVal = 
                (ArrayParameter<object, ValueVariable<object>, Parameter<object, ValueVariable<object>>>) 
                parameterValue;
        }
        else
        {
            if (parameterValue == null) 
                parameterValue = valueVariable.CreateNewArrayParameter();
            var parameterVal = (Parameter<object, ValueVariable<object>>) parameterValue;
        }
        Parameter newParameter = DrawFunctionParameter(variable, parameterType);
    }
    Parameter DrawFunctionParameter(Variable variable, Type valueType)
    {
        EditorGUILayout.Space(2); 
        bool isArray = valueType.IsArray;
        if (isArray)
            valueType = valueType.GetElementType();
        Type variableType = valueType; // TODO
        Rect parameterRect = EditorGUILayout.GetControlRect(hasLabel: true, height: 16f, EditorStyles.layerMaskField);
        parameterRect.x += guiOffsetX;
        parameterRect.width -= guiOffsetX;
        EditorGUI.LabelField(parameterRect, $"{isArray}  "+ variableType);
        return null;
    }
    
}
}