#if UNITY_EDITOR
using EasyInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace PlayBox
{
	[CustomEditor(typeof(State), true)]
	public class StateEditor : Editor
	{
		const string editorPrefsKey = "OpenedStates";
		public static readonly Color gray = new(0.5f, 0.5f, 0.5f);
		static GUIStyle _guiStyle = new();

		[SerializeField] List<State> openStates = new();

		static GUIStyle _leftButtonStyle;
		public static GUIStyle LeftButtonStyle =>
			_leftButtonStyle = _leftButtonStyle ?? new GUIStyle(GUI.skin.button)
			{ alignment = TextAnchor.MiddleLeft };


		static GUIStyle _rightLabelStyle;
		public static GUIStyle RightLabelStyle =>
			_rightLabelStyle = _rightLabelStyle ?? new GUIStyle(GUI.skin.label)
			{ alignment = TextAnchor.MiddleRight };


		static GUIStyle _middleLabelStyle;
		public static GUIStyle MiddleLabelStyle =>
			_middleLabelStyle = _middleLabelStyle ?? new GUIStyle(GUI.skin.label)
			{ alignment = TextAnchor.MiddleCenter };


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
			State state = (State)target;
			State parent = state?.ParentStateMachine;

			DrawDefaultInspector();

			// DrawMonoScript();

			// EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(state.stateColor)), new GUIContent("Color"));
			// Debug.Log(colorP.colorValue.a);
			if (parent != null)
			{
				GUI.enabled = false;
				EditorGUILayout.ObjectField("Parent State Machine", parent, typeof(State), allowSceneObjects: true);
				GUI.enabled = true;
			}

			DrawInnerStates(state, 0);
		}

		void DrawInnerStates(State state, int indent)
		{
			DrawRow(state, indent);
			// DON'T MAKE IT FOREACH
			if (state == null)
				return;

			for (int index = 0; index < state.InnerStates.Count; index++)
			{
				State innerState = state.InnerStates[index];
				if (openStates.Contains(state))
					DrawInnerStates(innerState, indent + 1);
			}
		}

		static void CreateInnerState(State state)
		{
			GameObject go = state.gameObject;
			GameObject childGO = new();
			State childState = (State)childGO.AddComponent(state.GetType());
			childState.stateColor = state.stateColor;
			childGO.name = "New State";
			childGO.transform.parent = go.transform;
			childGO.transform.localPosition = Vector3.zero;
			childGO.transform.localRotation = Quaternion.identity;
			childGO.transform.localScale = Vector3.one;
			state.UpdateState();
			Undo.RegisterCreatedObjectUndo(childGO, "Sub-State Created");
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
			const float colorFieldSize = 16;
			const float actionButtonWith = 22;
			float space = EditorGUIUtility.standardVerticalSpacing;

			Rect rowRect = EditorGUILayout.GetControlRect(true);
			float indentsWidth = indent * EditorHelper.indentWidth;

			// Toggle
			Rect toggleRect = new(rowRect.x + indentsWidth, rowRect.y, defaultToggleWith, rowRect.height);
			bool isDefault = state.IsDefaultSate;
			GUI.enabled = state.ParentStateMachine != null && !Application.isPlaying;
			string toolTip = isDefault ? "Default State" : "Not Default State";
			bool newDefault = GUI.Toggle(toggleRect, isDefault, new GUIContent(null, null, toolTip));

			if (isDefault != newDefault)
			{
				Undo.RecordObject(state.ParentStateMachine, "Default State Changed");
				if (state.IsDefaultSate)
					state.UnsetAsDefault();
				else
					state.SetAsDefault();
			}

			// COLOR
			Rect colorRect = new(toggleRect.x + colorFieldSize, rowRect.y + 1, colorFieldSize, colorFieldSize);
			Color newColor = EditorGUI.ColorField(colorRect, GUIContent.none, state.stateColor, false, false, false);
			newColor.a = 1;
			if (newColor != state.stateColor)
			{
				Undo.RecordObject(state, "Color Changed");
				state.stateColor = newColor;
			}

			float buttonWidth = rowRect.width - indentsWidth - (defaultToggleWith) - colorFieldSize - (2 * actionButtonWith) - (3 * space);
			Rect buttonRect = new(colorRect.xMax + space, rowRect.y, buttonWidth,
				rowRect.height);

			// INNER STATES 
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
				Rect typeRect = new(buttonRect.xMax + space, buttonRect.y, stateMachineTypeWidth, buttonRect.height);
				StateMachineType type = state.StateMachineType;
				StateMachineType newType = (StateMachineType)EditorGUI.EnumPopup(typeRect, type);
				if (type != newType)
				{
					Undo.RecordObject(state, "States Type Changed");
					state.StateMachineType = newType;
				}
			}

			GUI.enabled = state.ParentStateMachine != null;
			GUI.color = state.StateEditorColor();
			if (GUI.Button(buttonRect, state.name, LeftButtonStyle))
			{
				Undo.RecordObject(state.ParentStateMachine, "States Changed");
				if (state.IsSelectedState)
					state.DeselectState();
				else
					state.SelectState();
			}

			GUI.enabled = true;

			// STATE BUTTON
			if (state.HasInnerStates)
				GUI.Label(buttonRect, $"{state.InnerStates.Count} ", RightLabelStyle);

			GUI.color = Color.white;

			// ADD BUTTON
			Rect actionButtonRect = new(rowRect.xMax - (2 * actionButtonWith) - space, rowRect.y, actionButtonWith, rowRect.height);
			GUIContent insertContent = EditorGUIUtility.IconContent("CreateAddNew");
			insertContent.tooltip = "Add Inner State";
			if (GUI.Button(actionButtonRect, insertContent))
				CreateInnerState(state);

			// Delet BUTTON
			actionButtonRect.x = actionButtonRect.xMax + space;
			if (GUI.Button(actionButtonRect, GUIContent.none))
				Undo.DestroyObjectImmediate(state.gameObject);
			GUIContent deleteContent = EditorGUIUtility.IconContent("winbtn_win_close");
			deleteContent.tooltip = "Delete State";
			GUI.Label(actionButtonRect, deleteContent, MiddleLabelStyle);
		}



		static void DrawHierarchyIcon(int instance, Rect selectionRect)
		{
			GameObject gameObject = (GameObject)EditorUtility.InstanceIDToObject(instance);
			if (gameObject == null)
				return;
			State state = gameObject.GetComponent<State>();

			if (state == null)
				return;

			Vector2 textSize = _guiStyle.CalcSize(new GUIContent(gameObject.name));
			Rect iconRect = new(selectionRect)
			{
				x = selectionRect.x + textSize.x + 25,
				y = selectionRect.y - 1,
				width = EditorGUIUtility.singleLineHeight,
				height = EditorGUIUtility.singleLineHeight
			};
			GUIContent iconGUIContent = state.StateIcon();
			EditorGUI.LabelField(iconRect, iconGUIContent);
			if (state.IsDefaultSate && state.ParentStateMachine != null)
			{
				Rect defaultIconRect = new(iconRect) { x = iconRect.x + 14, y = iconRect.y - 0 };
				EditorGUI.LabelField(defaultIconRect, StateMachineIconHelper.defaultState);
			}
		}
	}
}
#endif