using System;
using MUtility;
using PlayBox;
using UnityEngine;
using UnityEngine.Serialization;

public class VariableTester : MonoBehaviour
{
    [Serializable]
    public struct MyStruct
    {
        public int myInt;
        public string myString;
    }
    
    [SerializeField] MyStruct myStruct;
    [SerializeField] BoolVariable boolVariable;
    [SerializeField] IntVariable intVariable;
    [SerializeField] FloatVariable floatVariable;
    [SerializeField] StringVariable stringVariable;
    [SerializeField] Vector2Variable v2Variable;
    [SerializeField] Vector3Variable v3Variable;
    [FormerlySerializedAs("triggerParameter")] [SerializeField] EventVariable eventVariable;
    [SerializeField] AnimationCurveVariable animationCurveVariable;
    [FormerlySerializedAs("directionParameter")] [SerializeField] DirectionVariable directionVariable;
    [SerializeField] DateTimeVariable dateTimeVariable;
    [SerializeField] [SearchEnum] KeyCode testSearchEnum;

    void OnEnable()
    {
        throw new NotImplementedException();
    }
}
