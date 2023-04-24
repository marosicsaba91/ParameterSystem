using System;
using UnityEngine;

namespace PlayBox
{
	[RequireComponent(typeof(State))]
	public abstract class StateTransition : MonoBehaviour
	{
		[SerializeField, HideInInspector] State state;

		internal State State
		{
			get
			{
				if (!Application.isPlaying)
					state = GetComponent<State>();
				return state;
			}
		}

		internal enum TransitionType
		{
			EnterToThisState,
			ExitFromThisState

		}

		[SerializeField, HideInInspector] internal TransitionType transitionType;
		[SerializeField, HideInInspector] internal State destination;

		void OnValidate()
		{
			state = GetComponent<State>();
		}

		protected virtual void OnEnable() { } // KEEP IT 

		protected void InvokeTransition()
		{
			if (!enabled)
				return;
			if (state == null || state.ParentStateMachine == null)
				return;
			switch (transitionType)
			{
				case TransitionType.EnterToThisState:
					state.SelectState();
					break;
				case TransitionType.ExitFromThisState:
					if (destination == null)
						state.DeselectState();
					else
						state.ParentStateMachine.TryChangeSelectedState(state, destination);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}