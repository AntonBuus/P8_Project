using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeftButtonInteractor : ButtonInteractorBase
{
    [Header("Left Button Settings")]
    [Tooltip("Event triggered when the button is pressed")]
    public UnityEvent onButtonPress;
    
    [Tooltip("Event triggered when the button is released")]
    public UnityEvent onButtonRelease;
    
    protected override void InitializeButton()
    {
        // Initialize any specific properties for this button type
        // Default implementation is empty
    }
    
    protected override bool CheckRaycastHit(RaycastHit hit)
    {
        // Simple check if the hit object is this button
        return hit.transform == transform;
    }
    
    protected override void OnLeftClickPressed(Vector2 mousePosition)
    {
        // Trigger the button press event
        onButtonPress?.Invoke();
        
        // Play sound for interaction feedback
        PlayInteractionSound();
        
        // Optional: Add visual feedback
        FlashMaterial();
        
        Debug.Log($"Button {gameObject.name} pressed");
    }
    
    protected override void OnLeftClickDrag(Vector2 mousePosition)
    {
        // For a simple button, we don't need any drag behavior
        // The base class handles tracking the pressed state
    }
    
    protected override void OnLeftClickReleased(Vector2 mousePosition)
    {
        // Trigger the button release event
        onButtonRelease?.Invoke();
        
        Debug.Log($"Button {gameObject.name} released");
    }
    
    protected override void OnRightClickPressed(Vector2 mousePosition)
    {
        // Default implementation - right click will be handled by the base class
        // including the physical press animation
        Debug.Log($"Button {gameObject.name} right-clicked");
    }
}
