 using UnityEngine;

namespace PlayBox
{
public class StateTransitionTrigger : StateTransition
{
	enum TriggerEvent
	{
		Enter,
		Exit
	}

	[SerializeField] TriggerEvent triggerEvent;
 
	void OnTriggerEnter(Collider other)
	{
		if(triggerEvent == TriggerEvent.Enter)
			InvokeTransition();
	}

	void OnTriggerExit(Collider other)
	{ 
		if(triggerEvent == TriggerEvent.Exit)
			InvokeTransition();
	}

}
}