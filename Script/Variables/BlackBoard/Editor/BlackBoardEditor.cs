#if UNITY_EDITOR
using EasyInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayBox
{
	[CustomEditor(typeof(BlackBoard))]
	public class BlackBoardEditor : Editor
	{
		static float _itemHeight;
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			SetupHeight();
			BlackBoard blackBoard = (BlackBoard)target;
			IEnumerable<Variable> variables = blackBoard.Variables;

			Rect position = EditorGUILayout.GetControlRect(hasLabel: false, GetHeight(blackBoard));

			EditorHelper.DrawBox(position);

			List<string> openedElements = blackBoard.openedVariables;
			blackBoard.scrollPosition = VariableTreeDrawer.DrawVariables(
				variables,
				openedElements,
				blackBoard.scrollPosition,
				position,
				VariableTree.DrawingType.MonoBehaviour);
		}

		static void SetupHeight()
		{
			_itemHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		}

		public float GetHeight(BlackBoard blackBoard)
		{
			VariableTree tree = new(blackBoard.Variables);
			List<string> openedElements = blackBoard.openedVariables;
			float treeHeight = VariableTreeDrawer.TreeHeight(tree, openedElements);
			SetupHeight();
			return Mathf.Min(_itemHeight, treeHeight + 1) + 1 + _itemHeight;
		}
	}
}
#endif