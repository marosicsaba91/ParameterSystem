using UnityEngine;
using Object = UnityEngine.Object;

namespace PlayBox
{
public class StateEffectActivate : StateEffect
{
	enum WhatToDo
	{
		Enable,
		Disable,
		EnableOnEnterDisableOnExitState,
		EnableOnExitDisableOnEnterState
	}

	[SerializeField] WhatToDo whatToDo;
	[Tooltip("Activates GameObjects, Enables Behaviours & Colliders")]
	[SerializeField] Object[] subjects;

	public override void InvokeEffect(bool isEntering, State state)
	{
		if(subjects == null) return;

		bool enable =
			whatToDo == WhatToDo.Enable ||
			(whatToDo == WhatToDo.EnableOnEnterDisableOnExitState && isEntering) ||
			(whatToDo == WhatToDo.EnableOnExitDisableOnEnterState && !isEntering);

		foreach (Object subject in subjects)
		{

			if (subject is GameObject go)
				go.SetActive(enable);
			else if (subject is Behaviour behaviour)
				behaviour.enabled = enabled;
			else if (subject is Collider coll)
				coll.enabled = enabled;
		}
	}
}
}