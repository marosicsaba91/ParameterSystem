#if UNITY_EDITOR
 
using UnityEditor; 

namespace StateMachineSystem
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