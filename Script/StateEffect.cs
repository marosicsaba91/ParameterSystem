using UnityEngine;

namespace PlayBox
{
[RequireComponent(typeof(State))]
public abstract class StateEffect : MonoBehaviour
{
	protected enum Trigger {OnEnter, OnExit, OnEnterAndExit}
	[SerializeField, HideInInspector] State state;

	[SerializeField] protected Trigger whenInvokeEffect; 
	[SerializeField] protected bool invokeOnAwake = true;

	public State State => state;

	void OnValidate()
	{
		state = GetComponent<State>();
	} 

	internal void OnStateEnter(State state)
	{
		if(!enabled) return;
		if (whenInvokeEffect == Trigger.OnEnter || whenInvokeEffect == Trigger.OnEnterAndExit)
			InvokeEffect(isEntering: true, state);
	}
	
	internal void OnStateExit(State state)
	{
		if(!enabled) return;
		if (whenInvokeEffect == Trigger.OnExit || whenInvokeEffect == Trigger.OnEnterAndExit)
			InvokeEffect(isEntering: false, state);
	}

	internal void InvokeEffectOnAwake(bool isEntering, State state)
	{
		if (invokeOnAwake)
			InvokeEffect(isEntering, state);
	}

	public abstract void InvokeEffect(bool isEntering, State state);
	 
	void OnEnable() { } // KEEP IT

}
}