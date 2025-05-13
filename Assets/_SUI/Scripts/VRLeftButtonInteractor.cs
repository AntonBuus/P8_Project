using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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

    // Reference to the XR interactable component
    private XRBaseInteractable xrInteractable;
    
    // Reference to the screen's renderer
    private Renderer screenRenderer;
    
    // Animation state tracking
    private Vector3 originalPosition;
    private bool isPressed = false;
    private Coroutine animationCoroutine;
    
    // Track screen power state
    private bool isScreenOn = false;

    private void Awake()
    {
        // Store original position for animation
        originalPosition = transform.localPosition;
        
        // First try to get the specific XRSimpleInteractable
        xrInteractable = GetComponent<XRSimpleInteractable>();
        
        // If that fails, try to get any XRBaseInteractable
        if (xrInteractable == null)
        {
            xrInteractable = GetComponent<XRBaseInteractable>();
        }
        
        if (xrInteractable == null)
        {
            Debug.LogError("No XR Interactable component found. Please add an XRSimpleInteractable component to this GameObject.");
            return;
        }
        
        // Register for XR Interaction events - we only need select entered as a toggle
        xrInteractable.selectEntered.AddListener(OnButtonPressed);
        xrInteractable.selectExited.AddListener(OnButtonReleased);
        
        // Get screen renderer
        if (screenObject != null)
        {
            screenRenderer = screenObject.GetComponent<Renderer>();
            if (screenRenderer == null)
            {
                Debug.LogWarning("Screen object does not have a Renderer component.");
            }
        }
        else
        {
            Debug.LogWarning("No screen object assigned to power button.");
        }
        
        // Initialize screen to OFF state
        UpdateScreenState();
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveListener(OnButtonPressed);
            xrInteractable.selectExited.RemoveListener(OnButtonReleased);
        }
    }

    private void Start()
    {
        Debug.Log("VR Button Interactor initialized");
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        Debug.Log($"Button {gameObject.name} pressed");
        
        // Toggle screen state
        ToggleScreenState();
        
        PlayInteractionSound();
        
        // Animate button press
        AnimateButtonPress(true);
    }
    
    private void OnButtonReleased(SelectExitEventArgs args)
    {
        Debug.Log($"Button {gameObject.name} released");
        
        // Animate button release
        AnimateButtonPress(false);
    }

    private void ToggleScreenState()
    {
        isScreenOn = !isScreenOn;
        UpdateScreenState();
        Debug.Log($"Screen power toggled: {(isScreenOn ? "ON" : "OFF")}");
    }
    
    private void UpdateScreenState()
    {
        if (screenObject == null || screenRenderer == null)
            return;
            
        // Update material based on screen state
        if (isScreenOn)
        {
            if (screenOnMaterial != null)
            {
                screenRenderer.material = screenOnMaterial;
            }
            
            // Ensure the screen object is active
            screenObject.SetActive(true);
        }
        else
        {
            if (screenOffMaterial != null)
            {
                screenRenderer.material = screenOffMaterial;
            }
            else
            {
                // If no OFF material is provided, just deactivate the object
                screenObject.SetActive(false);
            }
        }
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
}
