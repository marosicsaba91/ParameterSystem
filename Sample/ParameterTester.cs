using MUtility;
using PlayBox;
using UnityEngine;

public class ParameterTester : MonoBehaviour
{
    [SerializeField] BoolParameter boolParameter;
    [SerializeField] IntParameter intParameter;
    [SerializeField] FloatParameter floatParameter;
    [SerializeField] StringParameter stringParameter;
    [SerializeField] Vector2Parameter v2Parameter;
    [SerializeField] Vector3Parameter v3Parameter;
    [SerializeField] TriggerParameter triggerParameter;
    [SerializeField] [SearchEnum] KeyCode testSearchEnum;
}
