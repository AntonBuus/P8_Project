using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class VRLeftButtonInteractor : MonoBehaviour
{
    [Header("Screen Settings")]
    [Tooltip("The screen GameObject to toggle on/off")]
    [SerializeField] private GameObject screenObject;
    
    [Tooltip("Material to use when the screen is ON")]
    [SerializeField] private Material screenOnMaterial;
    
    [Tooltip("Material to use when the screen is OFF")]
    [SerializeField] private Material screenOffMaterial;

    [Header("Button Animation")]
    [Tooltip("How far the button moves when pressed")]
    [SerializeField] private float pressDepth = 0.005f;
    
    [Tooltip("Animation duration in seconds")]
    [SerializeField] private float pressDuration = 0.05f;
    
    [Tooltip("Animation curve for smoother transitions")]
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Axis along which the button moves when pressed")]
    [SerializeField] private PressAxis pressAxis = PressAxis.Y;
    
    public enum PressAxis { Y, Z }
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactionSound;

    [Header("Debug")]
    [SerializeField] private bool showPokeZone = false;
    private GameObject pokeVisualizer;

    [Tooltip("Enable detailed diagnostic logging to find what's triggering the button")]
    [SerializeField] private bool detailedDebugLogging = true;

    [Header("Interaction Settings")]
    [Tooltip("Which interaction methods this button should respond to")]
    [SerializeField] private InteractionMode interactionMode = InteractionMode.AllMethods; // Changed default to AllMethods

    public enum InteractionMode
    {
        PokeTrigger,    // Only trigger on poke
        HoverTrigger,   // Only trigger on hover
        SelectTrigger,  // Only trigger on select (including NearFarInteractor)
        AllMethods      // Respond to any interaction
    }

    // Animation state tracking
    private Vector3 originalPosition;
    private bool isPressed = false;
    private Coroutine animationCoroutine;
    
    // Track screen power state
    private bool isScreenOn = false;
    
    // Renderer for the screen
    private Renderer screenRenderer;

    // New fields for managing toggle state and preventing multiple toggles
    private bool isHovering = false;
    private float lastToggleTime = 0f;
    [SerializeField] private float toggleCooldown = 0.2f; // Prevents toggling more frequently than this

    private void Awake()
    {
        // Store original position for animation
        originalPosition = transform.localPosition;
        
        // Get screen renderer
        if (screenObject != null)
        {
            screenRenderer = screenObject.GetComponent<Renderer>();
            if (screenRenderer == null)
            {
                Debug.LogWarning("Screen object does not have a Renderer component. Will toggle visibility only.");
            }
        }
        else
        {
            Debug.LogError("No screen object assigned to power button. Button won't function correctly.");
        }
        
        // Initialize screen to OFF state
        UpdateScreenState();
    }

    private void Start()
    {
        Debug.Log("VR Button Interactor initialized");
        
        // Make sure we have an XR Simple Interactable
        if (!TryGetComponent<XRSimpleInteractable>(out var simpleInteractable))
        {
            Debug.LogError("Missing XRSimpleInteractable component. Please add one to this GameObject.");
        }
        else
        {
            // Make sure select events are registered properly
            if (simpleInteractable.selectEntered.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("<color=yellow>No select events registered in the Inspector. Adding programmatically.</color>");
                
                // Add the event listener programmatically if not set in inspector
                simpleInteractable.selectEntered.AddListener(OnButtonInteraction);
            }
        }
        
        // Create debug visualizer if enabled
        if (showPokeZone)
        {
            CreatePokeVisualizer();
        }
    }

    private void OnValidate()
    {
        // This method will only run in the editor and provides guidance for setup
        Debug.Log("VR Button Setup Guide:\n" +
                  "1. Adjust XR Poke Interactor on your controller:\n" +
                  "   - Reduce Poke Depth to 0.05-0.07m (currently " + 0.1f + "m)\n" +
                  "   - Reduce Poke Width to 0.005m (currently " + 0.0075f + "m)\n" +
                  "   - Reduce Poke Select Width to 0.01m (currently " + 0.015f + "m)\n" +
                  "   - Reduce Poke Hover Radius to 0.01m (currently " + 0.015f + "m)\n" +
                  "   - Reduce Poke Interaction Offset to 0.002m (currently " + 0.005f + "m)\n" +
                  "2. Make sure the Mesh Collider size closely matches the visual button");
    }

    // PUBLIC METHODS FOR DIRECT CONNECTION IN UNITY INSPECTOR

    /// <summary>
    /// Direct toggle function that can be called from UI events or buttons
    /// </summary>
    public void ToggleScreen()
    {
        // Don't allow toggle more frequently than the cooldown period
        if (Time.time - lastToggleTime < toggleCooldown)
            return;
        
        lastToggleTime = Time.time;
        
        Debug.Log("ToggleScreen called directly");
        
        // Toggle screen state
        isScreenOn = !isScreenOn;
        
        // Update visuals
        UpdateScreenState();
        
        // Play sound
        PlayInteractionSound();
        
        // Animate button press and release
        PressAndReleaseAnimation();
        
        Debug.Log($"Screen toggled to: {(isScreenOn ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// Called when hover first enters the button
    /// </summary>
    public void OnHoverEntered(HoverEnterEventArgs args = null)
    {
        if (detailedDebugLogging && args != null)
        {
            Debug.Log($"<color=yellow>HOVER ENTER from: {args.interactorObject.transform.name}</color>");
            Debug.Log($"<color=yellow>Interactor type: {args.interactorObject.GetType().Name}</color>");
        }
        
        // Only toggle if we're using hover or all methods
        if (interactionMode == InteractionMode.HoverTrigger || interactionMode == InteractionMode.AllMethods)
        {
            if (!isHovering)
            {
                isHovering = true;
                ToggleScreen();
            }
        }
        else
        {
            // Just update the hover state without triggering
            isHovering = true;
        }
    }
    
    /// <summary>
    /// Called when hover leaves the button
    /// </summary>
    public void OnHoverExited() 
    {
        isHovering = false;
        ReleaseButton(); // Make sure the button visually pops back up
    }
    
    /// <summary>
    /// Called when button is interacted with (via Inspector events)
    /// </summary>
    public void OnButtonInteraction(SelectEnterEventArgs args = null)
    {
        if (detailedDebugLogging && args != null)
        {
            Debug.Log($"<color=green>SELECT ENTER from: {args.interactorObject.transform.name}</color>");
            Debug.Log($"<color=green>Interactor type: {args.interactorObject.GetType().Name}</color>");
            
            // Check if this is the NearFarInteractor that's triggering the button
            bool isNearFarInteractor = args.interactorObject.transform.name.Contains("Near-Far");
            Debug.Log($"<color=cyan>Is NearFarInteractor: {isNearFarInteractor}</color>");
        }
        
        // Only toggle if we're using select trigger or all methods
        if (interactionMode == InteractionMode.SelectTrigger || 
            interactionMode == InteractionMode.AllMethods ||
            interactionMode == InteractionMode.PokeTrigger) // Include PokeTrigger since NearFar seems to be your alternative
        {
            // This is the critical part - make sure we're actually calling ToggleScreen
            ToggleScreen();
        }
    }

    // Add these diagnostic methods to see what's triggering
    void OnTriggerEnter(Collider other)
    {
        if (detailedDebugLogging)
            Debug.Log($"<color=blue>TRIGGER ENTER from: {other.name}</color>");
    }

    void OnTriggerExit(Collider other)
    {
        if (detailedDebugLogging)
            Debug.Log($"<color=blue>TRIGGER EXIT from: {other.name}</color>");
    }

    // PRIVATE IMPLEMENTATION

    private void UpdateScreenState()
    {
        if (screenObject == null)
            return;
            
        // Update material based on screen state
        if (isScreenOn)
        {
            // Activate screen
            screenObject.SetActive(true);
            
            // Apply ON material if available
            if (screenRenderer != null && screenOnMaterial != null)
            {
                screenRenderer.material = screenOnMaterial;
            }
        }
        else
        {
            // Apply OFF material if available
            if (screenRenderer != null && screenOffMaterial != null)
            {
                screenRenderer.material = screenOffMaterial;
            }
            else
            {
                // No OFF material, just deactivate
                screenObject.SetActive(false);
            }
        }
    }

    private void PressAndReleaseAnimation()
    {
        // Start press animation
        AnimateButtonPress(true);
        
        // Schedule release animation after a short delay
        Invoke(nameof(ReleaseButton), 0.1f);
    }
    
    private void ReleaseButton()
    {
        AnimateButtonPress(false);
    }

    // Animate button press down or release up
    private void AnimateButtonPress(bool press)
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
    private IEnumerator AnimateButtonPressCoroutine(bool press)
    {
        float elapsed = 0f;
        
        // Get the current position
        Vector3 startPosition = transform.localPosition;
        
        // Calculate pressed and released positions based on the selected axis
        Vector3 pressVector = Vector3.zero;
        switch (pressAxis)
        {
            case PressAxis.Y:
                pressVector = Vector3.down * pressDepth;  // Use down for Y-axis (negative Y)
                break;
            case PressAxis.Z:
                pressVector = Vector3.back * pressDepth;
                break;
        }
        
        // Calculate target positions
        Vector3 pressedPosition = originalPosition + pressVector;
        Vector3 releasedPosition = originalPosition;
        
        // Determine target position based on press state
        Vector3 targetPosition = press ? pressedPosition : releasedPosition;
        
        // Perform the animation
        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / pressDuration);
            
            // Apply animation curve for smooth movement
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            // Move the button
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            yield return null;
        }
        
        // Ensure we end at the exact target position
        transform.localPosition = targetPosition;
        animationCoroutine = null;
    }

    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }

    private void CreatePokeVisualizer()
    {
        pokeVisualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pokeVisualizer.name = "PokeZoneVisualizer";
        pokeVisualizer.transform.SetParent(transform);
        pokeVisualizer.transform.localPosition = Vector3.zero;
        
        // Match the size to the collider
        Collider col = GetComponent<Collider>();
        if (col is MeshCollider meshCol)
        {
            // For mesh colliders, use an approximation
            var bounds = meshCol.sharedMesh.bounds;
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            pokeVisualizer.transform.localScale = Vector3.one * maxSize;
        }
        else if (col is BoxCollider boxCol)
        {
            pokeVisualizer.transform.localScale = boxCol.size;
        }
        else if (col is SphereCollider sphereCol)
        {
            pokeVisualizer.transform.localScale = Vector3.one * sphereCol.radius * 2;
        }
        else if (col is CapsuleCollider capsuleCol)
        {
            pokeVisualizer.transform.localScale = new Vector3(
                capsuleCol.radius * 2,
                capsuleCol.height,
                capsuleCol.radius * 2);
        }
        
        // Make the visualizer transparent
        if (pokeVisualizer.TryGetComponent<Renderer>(out var renderer))
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0, 1, 0, 0.3f);
            renderer.material = mat;
        }
        
        // Remove collider from visualizer
        Destroy(pokeVisualizer.GetComponent<Collider>());
    }
    
    // Add this method for testing the button manually
    public void TestToggle()
    {
        Debug.Log("<color=purple>Test toggle called</color>");
        ToggleScreen();
    }
}
