using UnityEngine;

namespace PlayBox
{
	public class StateTransitionDelayed : StateTransition
	{
		[SerializeField] float delayInSeconds;

		float? _enterTime = null;

		protected override void OnEnable()
		{
			base.OnEnable();
			State.EnteredInThisState += EnteredInThisState;
			State.ExitedFromThisState += ExitedFromThisStateExit;
		}


		protected void OnDisable()
		{
			State.EnteredInThisState -= EnteredInThisState;
			State.ExitedFromThisState -= ExitedFromThisStateExit;
		}

		void EnteredInThisState(State previousState) => _enterTime = Time.time;
		void ExitedFromThisStateExit(State nextState) => _enterTime = null;

		void Update()
		{
			if (_enterTime == null)
				return;
			if (Time.time - _enterTime.Value < delayInSeconds)
				return;

			_enterTime = null;
			InvokeTransition();
		}
	}
}