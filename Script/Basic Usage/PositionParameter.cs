using System.Collections.Generic;
using PlayBox; 
using UnityEngine;

[ExecuteInEditMode]
public class PositionParameter : MonoBehaviour
{
    [LocalParameter, SerializeField] Vector3Parameter position; 
    [LocalParameter, SerializeField] Vector3Parameter localPosition;

    void OnValidate() => Setup();

    void Awake() => Setup();

    public void Setup()
    {
        if (position != null)
        { 
            position.otherObjectChangingWithParameter = new List<Object> { transform};
            position.valueChanged -= OnPositionChanged;
            position.valueChanged += OnPositionChanged;
        } 
        if (localPosition != null)
        {
            localPosition.otherObjectChangingWithParameter = new List<Object> { transform};
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
        if (position != null)
        {
            Vector3 pos = transform.position;
            if (pos != position.Value)
                position.Value = pos;
        }

        if (localPosition != null)
        {
            Vector3 pos = transform.localPosition;
            if (pos != localPosition.Value)
                localPosition.Value = pos;
        }
    } 
}
