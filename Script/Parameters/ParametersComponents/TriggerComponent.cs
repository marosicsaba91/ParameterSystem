using System;
using MUtility;
using UnityEngine;

namespace StateMachineSystem
{
public class TriggerComponent : ParameterComponent<TriggerParameter>
{
    public event Action trigger; 
    [SerializeField] TriggerButton triggerButton; 

    public void OnTriggered()
    {
        trigger?.Invoke();
    }

    [Serializable]
    class TriggerButton : InspectorButton<TriggerComponent>
    {
        protected override void OnClick(TriggerComponent parentObject) => 
            parentObject.OnTriggered();

        protected override string Text(TriggerComponent parentObject, string originalLabel) => "Trigger";
    } 
}
}