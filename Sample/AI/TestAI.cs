using System; 
using MUtility;
using PlayBox;
using UnityEngine;

public class TestAI : MonoBehaviour
{
    [Serializable] class InspectorFloat : InspectorFloat<TestAI> { }


    [Header("Parameters")] [SerializeField, Range(0, 1)]
    float relativeDistance;

    [SerializeField] InspectorFloat threatLevel = new InspectorFloat
    {
        getMaximum = ai => 1,
        getMinimum = ai => 0,
        getColor = ai => ColorHelper.GradientLerp( ai.threatLevel, new Color(0.66f, 1f, 0.36f),
            Color.white,new Color(1f, 0.89f, 0.62f), new Color(1f, 0.44f, 0.31f))
    };
    
    public float ThreatLevel => threatLevel;
} 