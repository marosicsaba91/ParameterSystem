#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using MUtility;
using UnityEditor;
using UnityEngine;

namespace PlayBox
{
	[CustomEditor(typeof(StateTransition), editorForChildClasses: true)]
	public class StateTransitionEditor : Editor
	{
		static readonly string[] dontIncludeMe = { EditorHelper.scriptPropertyName };

		static GUIContent _exitContent;
		static GUIContent _enterContent;

		void OnEnable()
		{
			_exitContent = EditorGUIUtility.IconContent("Profiler.LastFrame");
			_enterContent = EditorGUIUtility.IconContent("Profiler.FirstFrame");
		}

		public override void OnInspectorGUI()
		{
			EditorHelper.DrawScriptLine(serializedObject);

			var stateTransition = (StateTransition)target;
			State state = stateTransition?.State;
			State parentState = state?.ParentStateMachine;

			Undo.RecordObject(stateTransition, "StateTransition Changed");

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Transition", GUILayout.Width(EditorHelper.LabelWidth - 4));
			const float directionW = 40;
			Rect full = EditorGUILayout.GetControlRect(true, 0);
			int w1 = (int)((EditorHelper.ContentWidth(full) - directionW - 6) / 2f);
			float w2 = EditorHelper.ContentWidth(full) - (2 * w1) - 6;
			GUI.enabled = false;
			GUILayout.Button("This State", GUILayout.Width(w1));
			GUI.enabled = true;
			bool enter = stateTransition.transitionType == StateTransition.TransitionType.EnterToThisState;
			GUIContent directionContent = enter ? _enterContent : _exitContent;

			if (GUILayout.Button(directionContent, GUILayout.Width(w2)))
			{
				stateTransition.transitionType = enter
					? StateTransition.TransitionType.ExitFromThisState
					: StateTransition.TransitionType.EnterToThisState;
			}

			enter = stateTransition.transitionType == StateTransition.TransitionType.EnterToThisState;

			if (enter)
			{
				GUI.enabled = false;
				GUILayout.Button("Any State", GUILayout.Width(w1));
				GUI.enabled = true;
			}
			else
			{
				var options = new List<State> { null };
				int selectedIndex = 0;
				int index = 1;
				foreach (State s in parentState.SelectableInnerStates)
				{
					if (s == state)
						continue;
					if (!s.enabled)
						continue;

					options.Add(s);

					if (stateTransition.destination == s)
						selectedIndex = index;

					index++;
				}

				int newIndex = EditorGUILayout.Popup(selectedIndex,
					options.Select(s => s == null ? "- Exit -" : s.name).ToArray(),
					GUILayout.Width(w1));

				if (newIndex != selectedIndex)
					stateTransition.destination = options[newIndex];
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(6);

			DrawDefaultInspectorWithoutScriptLine();
		}

		public void DrawDefaultInspectorWithoutScriptLine()
		{
			DrawPropertiesExcluding(serializedObject, dontIncludeMe);
			serializedObject.ApplyModifiedProperties();
		}
	}
}

#endif