#if UNITY_EDITOR
using System.Collections.Generic;
using MUtility;
using UnityEditor; 
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PlayBox
{
[CustomEditor(typeof(State))]
public class StateEditor : Editor
{   
	const string editorPrefsKey = "OpenedStates";
	public static readonly Color gray = new Color(0.5f, 0.5f, 0.5f);
	static GUIStyle _guiStyle = new GUIStyle();
 
	[SerializeField] List<State> openStates = new List<State>();
 
	static GUIStyle _leftButtonStyle;
	public static GUIStyle LeftButtonStyle => 
		_leftButtonStyle = _leftButtonStyle ?? new GUIStyle(GUI.skin.button) 
			{alignment = TextAnchor.MiddleLeft};
	
	
	static GUIStyle _rightLabelStyle;
	public static GUIStyle RightLabelStyle => 
		_rightLabelStyle = _rightLabelStyle ?? new GUIStyle(GUI.skin.label) 
			{alignment = TextAnchor.MiddleRight};

	
	static GUIStyle _middleLabelStyle;
	public static GUIStyle MiddleLabelStyle => 
		_middleLabelStyle = _middleLabelStyle ?? new GUIStyle(GUI.skin.label) 
			{alignment = TextAnchor.MiddleCenter};

	
	void OnEnable()
	{
		EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyIcon;
		EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon; 
		_guiStyle = new GUIStyle(); 
		
		string data = EditorPrefs.GetString(
			editorPrefsKey, JsonUtility.ToJson(this, prettyPrint: false));
		JsonUtility.FromJsonOverwrite(data, this);
	} 

	public void SavePrefs()
	{ 
		string data = JsonUtility.ToJson(this, prettyPrint: false);
		EditorPrefs.SetString(editorPrefsKey, data);
	}


	public override void OnInspectorGUI()
	{
		var state = (State)target;
		State parent = state?.ParentStateMachine;

		DrawMonoScript();

		GUI.enabled = false;
		EditorGUILayout.ObjectField("Parent State Machine", parent, typeof(State), allowSceneObjects: true);
		GUI.enabled = true;

		EditorGUILayout.Space();
		DrawInnerStates(state, 0);
	}

	void DrawInnerStates(State state, int indent)
	{
		DrawRow(state, indent);
		// DON'T MAKE IT FOREACH
		if(state== null) return;
		for (var index = 0; index < state.InnerStates.Count; index++)
		{
			State inner = state.InnerStates[index];
			if (openStates.Contains(state))
				DrawInnerStates(inner, indent + 1);
		}
	} 

	static void CreateInnerState(State state)
	{ 
		GameObject go = state.gameObject;
		var child = new GameObject();
		child.AddComponent<State>();
		child.name = "New State";
		child.transform.parent = go.transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		child.transform.localScale = Vector3.one;
		state.UpdateState();
		Undo.RegisterCreatedObjectUndo(child,"Sub-State Created");
	}
 
	void DrawMonoScript()
	{
		GUI.enabled = false;
		SerializedProperty prop = serializedObject.FindProperty("m_Script");
		EditorGUILayout.PropertyField(prop, includeChildren: true);
		GUI.enabled = true;
	}

	void DrawRow(State state, int indent)
	{
		if (state == null)
			return;

		const float stateMachineTypeWidth = 120;
		const float defaultToggleWith = 16;
		const float actionButtonWith = 22;
		float space = EditorGUIUtility.standardVerticalSpacing;

		Rect rowRect = EditorGUILayout.GetControlRect(true);
		float indentsWidth = indent * EditorHelper.indentWidth;

		var toggleRect = new Rect(rowRect.x + indentsWidth, rowRect.y, defaultToggleWith, rowRect.height);
		bool isDefault = state.IsDefaultSate;
		GUI.enabled = state.ParentStateMachine != null && !Application.isPlaying;
		bool newDefault = GUI.Toggle(toggleRect, isDefault, new GUIContent(null, null, "Default State"));
		if (isDefault != newDefault)
		{ 
				Undo.RecordObject(state.ParentStateMachine, "Default State Changed");
				if (state.IsDefaultSate)
					state.UnsetAsDefault();
				else
					state.SetAsDefault();
		}

		float buttonWidth = rowRect.width - indentsWidth - defaultToggleWith - (2 * actionButtonWith) - (3 * space);
		var buttonRect = new Rect(toggleRect.xMax + space, rowRect.y, buttonWidth,
			rowRect.height);

		if (state.HasInnerStates)
		{
			GUI.enabled = true;
			bool open = openStates.Contains(state);
			bool newOpen = EditorGUI.Foldout(toggleRect, open, "");
			if (open != newOpen)
			{
				if (newOpen)
					openStates.Add(state);
				else
					openStates.Remove(state);
				SavePrefs();
			}

			buttonRect.width -= stateMachineTypeWidth + space;
			GUI.enabled = !Application.isPlaying;
			var typeRect = new Rect(buttonRect.xMax + space, buttonRect.y, stateMachineTypeWidth, buttonRect.height);
			StateMachineType type = state.StateMachineType;
			var newType = (StateMachineType)EditorGUI.EnumPopup(typeRect, type);
			if (type != newType)
			{
				Undo.RecordObject(state, "States Type Changed");
				state.StateMachineType = newType;
			}
		}

		GUI.enabled = state.ParentStateMachine != null;
		GUI.color = state.StateColor();
		if (GUI.Button(buttonRect, state.name, LeftButtonStyle))
		{
			Undo.RecordObject(state.ParentStateMachine, "States Changed");
			if (state.IsSelectedState)
				state.DeselectState();
			else
				state.SelectState();
		}

		GUI.enabled = true;

		if (state.HasInnerStates)
			GUI.Label(buttonRect, $"{state.InnerStates.Count} ", RightLabelStyle);

		GUI.color = Color.white;

		var actionButtonRect = new Rect(rowRect.xMax- (2 * actionButtonWith) - space, rowRect.y, actionButtonWith, rowRect.height);
		GUIContent insertContent = EditorGUIUtility.IconContent("CreateAddNew");
		insertContent.tooltip = "Insert Inner State.";
		if (GUI.Button(actionButtonRect, insertContent))
			CreateInnerState(state);

		actionButtonRect.x = actionButtonRect.xMax + space;
		if (GUI.Button(actionButtonRect, GUIContent.none))
			Undo.DestroyObjectImmediate(state.gameObject);
		GUIContent deleteContent = EditorGUIUtility.IconContent("winbtn_win_close");
		deleteContent.tooltip = "Delete State";
		GUI.Label(actionButtonRect, deleteContent, MiddleLabelStyle);
	}


	
	static void DrawHierarchyIcon(int instance, Rect selectionRect)
	{
		var gameObject = (GameObject) EditorUtility.InstanceIDToObject(instance);
		if(gameObject == null) return; 
		var state = gameObject.GetComponent<State>(); 
		
		if(state == null)
			return;
		
		Vector2 textSize =_guiStyle.CalcSize(new GUIContent(gameObject.name));
		var iconRect = new Rect(selectionRect)
		{
			x = selectionRect.x + textSize.x + 25,
			y = selectionRect.y - 1,
			width = EditorGUIUtility.singleLineHeight,
			height = EditorGUIUtility.singleLineHeight
		};
		GUIContent iconGUIContent = state.StateIcon();
		EditorGUI.LabelField(iconRect, iconGUIContent);
		if (state.IsDefaultSate && state.ParentStateMachine!=null)
		{ 
			var defaultIconRect = new Rect(iconRect) { x = iconRect.x + 14 , y = iconRect.y - 0 }; 
			EditorGUI.LabelField(defaultIconRect, StateMachineIconHelper.defaultState);
		}
	}
}
}
#endif