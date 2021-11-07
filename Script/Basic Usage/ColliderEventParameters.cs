using System;
using PlayBox; 
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class ColliderEventParameters : MonoBehaviour
{
    [Serializable]
    struct ColliderEvents
    {
        [LocalVariable] public EventVariable onCollisionEnter;
        [LocalVariable] public EventVariable onCollisionExit;
        [LocalVariable] public EventVariable onTriggerEnter;
        [LocalVariable] public EventVariable onTriggerExit;
    }

    [FormerlySerializedAs("colliderTriggers")] [SerializeField] ColliderEvents colliderEvents;

    [Serializable]
    struct Collider2DEvents
    {
        [LocalVariable] public EventVariable onCollisionEnter2D;
        [LocalVariable] public EventVariable onCollisionExit2D;
        [LocalVariable] public EventVariable onTriggerEnter2D;
        [LocalVariable] public EventVariable onTriggerExit2D;
    }

    [FormerlySerializedAs("collider2DTriggers")] [SerializeField] Collider2DEvents collider2DEvents;

    void OnCollisionEnter() => colliderEvents.onCollisionEnter?.InvokeEvent();
    void OnCollisionExit() => colliderEvents.onCollisionExit?.InvokeEvent();
    void OnTriggerEnter(Collider other) => colliderEvents.onTriggerEnter?.InvokeEvent();
    void OnTriggerExit(Collider other) => colliderEvents.onTriggerExit?.InvokeEvent();

    void OnCollisionEnter2D() => collider2DEvents.onCollisionEnter2D?.InvokeEvent();
    void OnCollisionExit2D() => collider2DEvents.onCollisionExit2D?.InvokeEvent();
    void OnTriggerEnter2D(Collider2D other) => collider2DEvents.onTriggerEnter2D?.InvokeEvent();
    void OnTriggerExit2D(Collider2D other) => collider2DEvents.onTriggerExit2D?.InvokeEvent();
}