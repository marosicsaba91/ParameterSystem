#if UNITY_EDITOR
using MUtility;
using UnityEditor;
using UnityEngine;

namespace StateMachineSystem
{
static class StateMachineIconHelper
{
    public static readonly Color gray = new Color(0.5f, 0.5f, 0.5f);

    static StateType GetStateType(this State state)
    {
        if (state == null) return StateType.Inactive;
        bool selected = state.IsSelectedState;
        bool selectable = state.IsSelectableState;

        if (selected)
            return selectable ? StateType.Selected : StateType.SelectedButInactive;
        return selectable ? StateType.Selectable : StateType.Inactive;
    }

    internal static GUIContent idleState;
    internal static GUIContent selectedState;
    internal static GUIContent inactiveState;
    internal static GUIContent selectedButInactiveState;
     
    internal static GUIContent idleStateMachine;
    internal static GUIContent selectedStateMachine;
    internal static GUIContent inactiveStateMachine;
    internal static GUIContent selectedButInactiveStateMachine; 
     
    internal static GUIContent defaultState;
    internal static GUIContent errorState;
    static void ImportIcons()
    {
        if(idleState!=null) return;
        

        idleState = AssetToGUIContent("State Selectable","Selectable State");
        selectedState = AssetToGUIContent("State Selected Active","Selected State");
        inactiveState = AssetToGUIContent("State Inactive","Inactive State");
        selectedButInactiveState =
            AssetToGUIContent("State Selected But Inactive", "Selected But Inactive State"); 
        
        idleStateMachine = AssetToGUIContent("State Machine Selectable","Selectable State Machine");
        selectedStateMachine = AssetToGUIContent("State Machine Selected Active","Selected State Machine");
        inactiveStateMachine = AssetToGUIContent("State Machine Inactive","Inactive State Machine");
        selectedButInactiveStateMachine =
            AssetToGUIContent("State Machine Selected But Inactive", "Selected But Inactive State Machine");
        defaultState = AssetToGUIContent("Default State Icon","Default State");
             
        errorState = EditorGUIUtility.IconContent("winbtn_mac_close");
        errorState.tooltip = "Error in State";
    }

    static GUIContent AssetToGUIContent(string iconName, string tooltip)
    { 
        string[] paths = AssetDatabase.FindAssets($"\"{iconName}\"");
        if (paths.Length <= 0) return new GUIContent(text: null, image: null, tooltip);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(paths[0]));
        return new GUIContent(texture, tooltip);
    }

    public static Color StateColor(this State state) => state.GetStateType().StateColor();

    static Color StateColor(this StateType stateType)
    {
        switch (stateType)
        {
            case StateType.Selectable:
                return Color.white;  
            case StateType.Selected:
                return EditorHelper.successGreenColor;
            case StateType.SelectedButInactive:
                return new Color(0.59f, 0.65f, 0.45f);
            case StateType.Inactive:
                return new Color(0.59f, 0.59f, 0.59f);
            default:
                return Color.red;
        }
    }
    
    public static GUIContent StateIcon(this State state)
    {
        ImportIcons();
        StateType stateType = state.GetStateType();
        
        if (state.ParentStateMachine == null && state.IsSelectedState)
            return idleStateMachine;
        
        if (state.HasInnerStates)
        {
            switch (stateType)
            {
                case StateType.Selected:
                    return selectedStateMachine;
                case StateType.Selectable:
                    return idleStateMachine;
                case StateType.Inactive:
                    return inactiveStateMachine;
                case StateType.SelectedButInactive:
                    return selectedButInactiveStateMachine;
                default:
                    return errorState;
            }
        }

        switch (stateType)
        {
            case StateType.Selected:
                return selectedState;
            case StateType.Selectable:
                return idleState;
            case StateType.Inactive:
                return inactiveState;
            case StateType.SelectedButInactive:
                return selectedButInactiveState;
            default:
                return errorState;
        }
    }
}

}
#endif