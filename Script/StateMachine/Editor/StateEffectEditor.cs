#if UNITY_EDITOR
 
using UnityEditor; 

namespace PlayBox
{ 
[CustomEditor(typeof(StateEffect), editorForChildClasses:true)]
public class StateEffectEditor : Editor
{   
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
	}
 
}
}

#endif