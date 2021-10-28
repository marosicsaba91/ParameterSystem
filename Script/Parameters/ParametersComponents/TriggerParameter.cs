using System;
using MUtility;
using UnityEngine;

namespace PlayBox
{
public class TriggerParameter : Parameter
{
    public event Action trigger; 
    [SerializeField] TriggerButton triggerButton; 

    public void OnTriggered()
    {
        trigger?.Invoke();
    }

    [Serializable]
    class TriggerButton : InspectorButton<TriggerParameter>
    {
        protected override void OnClick(TriggerParameter parentObject) => 
            parentObject.OnTriggered();

        protected override string Text(TriggerParameter parentObject, string originalLabel) => "Trigger";
    } 
}
}