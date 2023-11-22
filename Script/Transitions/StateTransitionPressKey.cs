using EasyInspector;
using UnityEngine;

namespace PlayBox
{
	public class StateTransitionPressKey : StateTransition
	{
		[SerializeField, SearchEnum] KeyCode keyCode;

		void Update()
		{
			if (Input.GetKeyDown(keyCode))
				InvokeTransition();
		}
	}
}