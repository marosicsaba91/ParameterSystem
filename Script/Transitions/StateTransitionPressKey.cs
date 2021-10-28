 using MUtility; 
 using UnityEngine;

namespace StateMachineSystem
{
public class StateTransitionPressKey : StateTransition
{
	[SerializeField, SearchEnum] KeyCode keyCode;

	void Update()
	{
		if(Input.GetKeyDown(keyCode))
			InvokeTransition();
	}
}
}