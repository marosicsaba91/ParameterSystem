using PlayBox;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    [SerializeField] TestAI ai;
    
    [Header("State")] 
    [SerializeField] State normal;
    [SerializeField] State alerted;
    [SerializeField] State hunting;

    void OnValidate()
    {
        if (ai == null)
            ai = GetComponentInParent<TestAI>();
    }

    void Update()
    {
        if (normal.IsSelectedState)
            UpdateNormal();
        else if (alerted.IsSelectableState)
            UpdateAlerted();
        else if (alerted.IsSelectableState)
            UpdateHunting();
            
    }
    
    void UpdateNormal()
    {
        if(ai.ThreatLevel > 0.5)
            alerted.SelectState();
    }
    
    void UpdateAlerted()
    {
        if(ai.ThreatLevel < 0.5)
            normal.SelectState();
        else if(ai.ThreatLevel > 0.75)
            hunting.SelectState();
    }

    void UpdateHunting()
    {
        if(ai.ThreatLevel < 0.75)
            alerted.SelectState();
    }
}
