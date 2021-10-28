using UnityEngine;

namespace StateMachineSystem
{
public class StateEffectLog : StateEffect
{
	public string specialMessage = string.Empty;
	public override void InvokeEffect(bool isEntering, State state)
	{
		Debug.Log($"{(isEntering ? "Enter" : "Exit")}: ({State.name})      {specialMessage}");
	}
 
}
}