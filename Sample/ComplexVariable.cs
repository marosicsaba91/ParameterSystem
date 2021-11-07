using System;
using UnityEngine;

namespace PlayBox
{ 
    [Serializable]
    public struct ComplexStruct
    {
        public int intValue;
        public bool boolValue;
        public AnimationCurve animCurve;
    }

    public class ComplexVariable : ValueVariable<ComplexStruct>
    {

    }
}