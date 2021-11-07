using MUtility;
using PlayBox;
using UnityEngine;

public class TestScript : MonoBehaviour
{ 
    [PlayBoxFunction]
    static float Multiply(params float[] values) => values.Aggregate((float acc, float value) => acc * value);
}