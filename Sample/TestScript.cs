using System.Linq;
using PlayBox;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	[PlayBoxFunction]
	static float Multiply(params float[] values) => values.Aggregate((acc, value) => acc * value);
}