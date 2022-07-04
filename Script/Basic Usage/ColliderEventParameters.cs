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
        [Parameter] public EventVariable onCollisionEnter;
        [Parameter] public EventVariable onCollisionExit;
        [Parameter] public EventVariable onTriggerEnter;
        [Parameter] public EventVariable onTriggerExit;
    }

    [FormerlySerializedAs("colliderTriggers")] [SerializeField] ColliderEvents colliderEvents;

    [Serializable]
    struct Collider2DEvents
    {
        [Parameter] public EventVariable onCollisionEnter2D;
        [Parameter] public EventVariable onCollisionExit2D;
        [Parameter] public EventVariable onTriggerEnter2D;
        [Parameter] public EventVariable onTriggerExit2D;
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