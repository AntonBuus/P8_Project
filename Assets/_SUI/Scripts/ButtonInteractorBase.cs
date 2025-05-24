using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

public abstract class ButtonInteractorBase : MonoBehaviour
{
    [Header("Base Button Settings")]
    [Tooltip("Enable or disable interaction with this button")]
    public bool interactable = true;
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip interactionSound;
    
    [Header("Right-Click Interaction")]
    [Tooltip("Event triggered when right-clicking the button")]
    public UnityEvent onRightClick;
    
    [Header("Visual Feedback")]
    public Material normalMaterial;
    public Material clickedMaterial;
    public float materialFlashDuration = 0.2f;
    protected MeshRenderer meshRenderer;
    private Coroutine materialFlashCoroutine;
    
    [Header("Press Animation")]
    [Tooltip("How far the button moves when pressed")]
    public float pressDepth = 0.005f;
    [Tooltip("Animation duration in seconds")]
    public float pressDuration = 0.05f;
    [Tooltip("Animation curve for smoother transitions")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Axis along which the button moves when pressed")]
    public enum PressAxis { Y, Z }
    public PressAxis pressAxis = PressAxis.Z;

    // Animation state
    protected bool isAnimating = false;
    protected Coroutine animationCoroutine;
    protected Vector3 originalPosition;
    protected bool isPressed = false;

    protected virtual void Awake()
    {
        // Store original position for animation
        originalPosition = transform.localPosition;
        
        // Get the mesh renderer component
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning($"No MeshRenderer found on {gameObject.name}. Material flash effect won't work.");
        }
        else if (normalMaterial == null)
        {
            // If no normal material is assigned, use the current material
            normalMaterial = meshRenderer.material;
        }
    }

    protected virtual void Start()
    {
        InitializeButton();
    }

    protected virtual void Update()
    {
        if (interactable)
        {
            HandleInput();
        }
    }

    // Method to be implemented by derived classes
    protected abstract void InitializeButton();

    // Method to be implemented by derived classes
    protected abstract bool CheckRaycastHit(RaycastHit hit);

    // Method to be implemented by derived classes
    protected abstract void OnLeftClickPressed(Vector2 mousePosition);

    // Method to be implemented by derived classes
    protected abstract void OnLeftClickDrag(Vector2 mousePosition);
    
    // Method to be implemented by derived classes
    protected abstract void OnLeftClickReleased(Vector2 mousePosition);
    
    // Method to be implemented by derived classes
    protected abstract void OnRightClickPressed(Vector2 mousePosition);

    // Shared input handling
    protected virtual void HandleInput()
    {
        // Check for input device
        if (Mouse.current == null) return;
        Mouse mouse = Mouse.current;
        
        Vector2 mousePosition = mouse.position.ReadValue();
        if (Camera.main == null) return;

        // Handle left button press
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && CheckRaycastHit(hit))
            {
                // Don't animate button press for left click, just call the handler
                isPressed = true; // Set pressed state without animation
                OnLeftClickPressed(mousePosition);
            }
        }
        
        // Handle left button release
        if (mouse.leftButton.wasReleasedThisFrame && isPressed)
        {
            // No animation on release for left click
            isPressed = false;
            OnLeftClickReleased(mousePosition);
        }
        
        // Handle right button press - with animation
        if (mouse.rightButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && CheckRaycastHit(hit))
            {
                // Only animate button press for right click
                AnimateButtonPress(true);
                OnRightClickPressed(mousePosition);
                TriggerRightClickAction();
                
                // Don't start the release coroutine here - we'll handle it separately when the button is released
            }
        }
        
        // New section to handle right-click release
        if (mouse.rightButton.wasReleasedThisFrame && isPressed)
        {
            // Only animate button release when right mouse button is released
            AnimateButtonPress(false);
        }
        
        // Handle dragging (if button is pressed)
        if (isPressed && mouse.leftButton.isPressed)
        {
            OnLeftClickDrag(mousePosition);
        }
    }
    
    // Animate button press down or release up
    protected virtual void AnimateButtonPress(bool press)
    {
        isPressed = press;
        
        // Stop any ongoing animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // Start press animation
        animationCoroutine = StartCoroutine(AnimateButtonPressCoroutine(press));
    }
    
    // Coroutine for button press animation
    protected virtual IEnumerator AnimateButtonPressCoroutine(bool press)
    {
        isAnimating = true;
        float elapsed = 0f;
        
        // Get the current position
        Vector3 startPosition = transform.localPosition;
        
        // Calculate pressed and released positions based on the selected axis
        Vector3 pressVector = Vector3.zero;
        switch (pressAxis)
        {
            case PressAxis.Y:
                pressVector = Vector3.down * pressDepth;
                break;
            case PressAxis.Z:
                // Force positive Z direction
                pressVector = Vector3.forward * pressDepth;
                break;
        }
        
        // Calculate target positions
        Vector3 pressedPosition = originalPosition + pressVector;
        Vector3 releasedPosition = originalPosition;
        
        // Set target position directly - only have it return on release, no animation on press
        if (press) {
            // Immediately set position when pressing
            transform.localPosition = pressedPosition;
            isAnimating = false;
            animationCoroutine = null;
            yield break; // End the coroutine instead of trying to return null
        }
        
        // Only animate when releasing
        Vector3 endPosition = releasedPosition;
        
        Debug.Log($"Button animation: {gameObject.name}, Press: {press}, Start: {startPosition}, End: {endPosition}, Original: {originalPosition}, Axis: {pressAxis}, PressVector: {pressVector}");
        
        // Perform the animation (only for release)
        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / pressDuration);
            
            // Apply animation curve for smooth movement
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            // Move the button - directly interpolate between start and end
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
            
            yield return null;
        }
        
        // Ensure we end at the exact target position
        transform.localPosition = endPosition;
        isAnimating = false;
        animationCoroutine = null;
    }
    
    // Trigger the right-click event and visual feedback
    protected virtual void TriggerRightClickAction()
    {
        onRightClick?.Invoke();
        PlayInteractionSound();
        FlashMaterial();
    }
    
    // Play sound effect
    protected virtual void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    
    // Flash material for visual feedback
    protected virtual void FlashMaterial()
    {
        if (meshRenderer != null && clickedMaterial != null)
        {
            // Stop any ongoing material flash
            if (materialFlashCoroutine != null)
            {
                StopCoroutine(materialFlashCoroutine);
            }
            
            // Start new flash coroutine
            materialFlashCoroutine = StartCoroutine(MaterialFlashCoroutine());
        }
    }
    
    // Coroutine for material flash effect
    protected virtual IEnumerator MaterialFlashCoroutine()
    {
        // Store the original material
        Material originalMaterial = meshRenderer.material;
        
        // Switch to clicked material
        meshRenderer.material = clickedMaterial;
        
        // Wait for the flash duration
        yield return new WaitForSeconds(materialFlashDuration);
        
        // Restore original material
        meshRenderer.material = normalMaterial;
        
        materialFlashCoroutine = null;
    }
    
    // Generic animation coroutine that can be used by derived classes
    protected virtual IEnumerator AnimateCoroutine(System.Action<float> updateAction, System.Action onComplete = null)
    {
        isAnimating = true;
        float elapsed = 0f;
        
        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / pressDuration);
            
            // Apply animation curve for smooth movement
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            // Update using the provided action
            updateAction(curveValue);
            
            yield return null;
        }
        
        // Ensure we complete the animation properly
        updateAction(1.0f);
        isAnimating = false;
        
        // Call the completion callback if provided
        onComplete?.Invoke();
    }
    
    // Stop any ongoing animation
    protected virtual void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            isAnimating = false;
        }
    }
}
