using System.Collections.Generic;
using PlayBox; 
using UnityEngine;

[ExecuteInEditMode]
public class TransformParameters : MonoBehaviour
{
    [LocalVariable, SerializeField] Vector3Variable position; 
    [LocalVariable, SerializeField] Vector3Variable localPosition; 
    [LocalVariable, SerializeField] Vector3Variable velocity;

    Vector3 _lastPos = new Vector3();

    void OnValidate() => Setup();

    void Awake() => Setup();

    void Setup()
    {
        if (position != null)
        { 
            position.otherObjectChangingWithVariable = new List<Object> { transform };
            position.valueChanged -= OnPositionChanged;
            position.valueChanged += OnPositionChanged;
        } 
        if (localPosition != null)
        {
            localPosition.otherObjectChangingWithVariable = new List<Object> { transform };
            localPosition.valueChanged -= OnLocalPositionChanged;
            localPosition.valueChanged += OnLocalPositionChanged;
        }
    }
    
    public void OnPositionChanged(Vector3 old, Vector3 newValue)
    {
        transform.position = newValue;
    } 
    
    public void OnLocalPositionChanged(Vector3 old, Vector3 newValue)
    {
        transform.localPosition = newValue;
    } 
    
    public void Update()
    {
        Vector3 pos = transform.position;
        if (position != null)
        { 
            if (pos != position.Value)
                position.Value = pos;
        }

        if (localPosition != null)
        {
            Vector3 localPos = transform.localPosition;
            if (localPos != localPosition.Value)
                localPosition.Value = localPos;
        }

        Vector3 movement = pos - _lastPos;
        _lastPos = pos;
        
        if (velocity != null)
            velocity.Value = movement / Time.deltaTime; 
    } 
}
