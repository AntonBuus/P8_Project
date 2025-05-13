using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class VRThumbButtonInteractor : MonoBehaviour
{
    [Header("Opacity Control")]
    [Tooltip("Minimum opacity value (0-1)")]
    [Range(0f, 1f)]
    public float minOpacity = 0f;
    
    [Tooltip("Maximum opacity value (0-1)")]
    [Range(0f, 1f)]
    public float maxOpacity = 1f;
    
    [Tooltip("How sensitive the control is to hand movement")]
    [Range(0.1f, 10f)]
    public float sensitivity = 2f;
    
    [Header("Position Mapping Settings")]
    [Tooltip("Distance scale for mapping hand position to opacity")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float distanceScale = 0.05f;
    
    [Tooltip("Reference transform for stable directional mapping (should not rotate with wheel)")]
    [SerializeField] private Transform referenceTransform;
    
    [Header("References")]
    [Tooltip("The GameObject whose opacity will be controlled")]
    [SerializeField] private GameObject hologram;
    [SerializeField] private GameObject hologramEmitter;
    [SerializeField] private GameObject hologramLight;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactionSound;
    
    [Header("Visual Feedback")]
    [SerializeField] private float knobRotationMax = 180f;
    [SerializeField] private bool showDebugGizmos = false;
    
    // Private configuration
    private string opacityPropertyName = "_Opacity";
    private string emissionPropertyName = "_EmissionColor";
    private int materialIndex = 0;
    private float minEmissionIntensity = 0f;
    private float maxEmissionIntensity = 1f;
    private float minLightIntensity = 0f;
    private float maxLightIntensity = 0.02f;
    
    // Interaction state
    private float currentOpacity = 1f;
    private float targetOpacity = 1f;
    private Coroutine animationCoroutine;
    
    // XR Simple Interactable
    private XRSimpleInteractable interactable;
    private Transform interactingHand;
    private bool isInteracting = false;
    
    // Hand position tracking for stability
    private Vector3 initialHandPositionRelative;
    private Vector3 lastHandPosition;
    private float lastProjectedDist = 0f;
    private float projectionSmoothing = 0.2f; // Lower = less smooth, higher = smoother
    
    // Audio timing
    private float lastAudioTime = 0f;
    private float audioDelay = 0.2f; // Minimum time between audio cues
    
    // For debugging
    private bool debugMode = false;
    private int debugFrameCounter = 0;
    private const int DEBUG_LOG_INTERVAL = 60; // Log every 60 frames
    
    void Awake()
    {
        // Get or add the interactable component
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
            Debug.Log("Added XRSimpleInteractable component");
        }
        
        // Make sure we have a collider
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.05f, 0.05f); // Small default size
            Debug.Log("Added BoxCollider component");
        }
        
        // Register for interaction events
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }
    
    void Start()
    {
        // Initialize the objects if not assigned
        if (hologram == null)
        {
            hologram = gameObject;
            Debug.LogWarning("No hologram object assigned, using this gameObject instead.");
        }
        
        // Check for reference transform
        if (referenceTransform == null)
        {
            Debug.LogWarning("No reference transform assigned, using this transform's parent instead.");
            if (transform.parent != null)
            {
                referenceTransform = transform.parent;
            }
            else
            {
                Debug.LogWarning("No parent transform found. Using this transform, which may cause rotation issues.");
                referenceTransform = transform;
            }
        }
        
        // Set initial opacity
        SetOpacityValue(maxOpacity, false);
    }
    
    void Update()
    {
        if (isInteracting && interactingHand != null)
        {
            // Calculate opacity based on hand position relative to the button
            CalculateOpacityFromHandPosition();
            
            // Debug logging at intervals
            if (debugMode)
            {
                debugFrameCounter++;
                if (debugFrameCounter % DEBUG_LOG_INTERVAL == 0)
                {
                    Debug.Log($"Opacity: {currentOpacity:F2}, Last projected dist: {lastProjectedDist:F3}");
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
    }
    
    // Calculate opacity from the hand position relative to the reference transform's up axis
    private void CalculateOpacityFromHandPosition()
    {
        // Early out if we have an invalid hand reference
        if (interactingHand == null) return;
        
        // Get current hand position
        Vector3 handPos = interactingHand.position;
        
        // Calculate the vector from the button to the hand
        Vector3 buttonToHand = handPos - transform.position;
        
        // Project hand position onto ONLY the reference transform's up axis
        // This ensures we use a stable direction that doesn't rotate with the thumbwheel
        Vector3 referenceAxis = referenceTransform.up;
        
        // Invert the projection to make the direction feel more natural
        // This makes moving up increase opacity and moving down decrease opacity
        float rawProjectedDist = -Vector3.Dot(buttonToHand, referenceAxis) * sensitivity;
        
        // Initial offset adjustment using the relative hand position at the start of interaction
        float initialOffset = -Vector3.Dot(initialHandPositionRelative, referenceAxis) * sensitivity;
        
        // Calculate the current projection with the initial offset removed
        float currentHandProjection = rawProjectedDist - initialOffset;
        
        // Apply additional smoothing to reduce jitter
        float currentSmoothing = Mathf.Lerp(0.5f, 0.1f, Mathf.Abs(currentHandProjection - lastProjectedDist) * 10f);
        float smoothedProjectedDist = Mathf.Lerp(lastProjectedDist, currentHandProjection, 1 - currentSmoothing);
        
        // Store for next frame's smoothing
        lastProjectedDist = smoothedProjectedDist;
        
        // Visualize projection in debug mode
        if (showDebugGizmos)
        {
            Debug.DrawLine(transform.position, transform.position + referenceAxis * distanceScale, Color.green, 0.01f);
            Debug.DrawLine(transform.position, transform.position + referenceAxis * smoothedProjectedDist, Color.yellow, 0.01f);
        }
        
        // Map projected distance to opacity range
        float normalizedDistance = Mathf.InverseLerp(-distanceScale/2, distanceScale/2, smoothedProjectedDist);
        normalizedDistance = Mathf.Clamp01(normalizedDistance);
        
        // Invert the opacity mapping to match rotation direction
        // When rotation is 0 (down), opacity should be min
        // When rotation is maxRotation (up), opacity should be max
        float newOpacity = Mathf.Lerp(maxOpacity, minOpacity, normalizedDistance);
        
        // Apply the opacity change if significant
        if (Mathf.Abs(newOpacity - currentOpacity) > 0.01f)
        {
            // Update the opacity - no animation during interaction
            SetOpacityValue(newOpacity, false);
            
            // Play sound occasionally for feedback
            if (Mathf.Abs(newOpacity - currentOpacity) > 0.05f && Time.time - lastAudioTime > audioDelay)
            {
                PlayInteractionSound();
                lastAudioTime = Time.time;
            }
        }
        
        // Update button rotation to match the opacity
        UpdateButtonRotation();
    }
    
    // Update the physical rotation of the button based on current opacity
    private void UpdateButtonRotation()
    {
        // Calculate rotation angle based on opacity (map 0-1 to 0-maxRotation)
        float normalizedOpacity = Mathf.InverseLerp(minOpacity, maxOpacity, currentOpacity);
        
        // Apply rotation directly without inversion
        // As opacity increases, rotation increases (0 to maxRotation)
        float rotationAngle = normalizedOpacity * knobRotationMax;
        
        // Always rotate around X axis for the thumbwheel
        transform.localRotation = Quaternion.Euler(rotationAngle, 0, 0);
    }
    
    // Handler for when interaction begins
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Ensure we're not already interacting
        if (isInteracting) return;
        
        isInteracting = true;
        interactingHand = args.interactorObject.transform;
        
        if (interactingHand == null)
        {
            Debug.LogError("Interaction started but hand reference is null!");
            isInteracting = false;
            return;
        }
        
        // Properly store initial hand position relative to button
        initialHandPositionRelative = interactingHand.position - transform.position;
        lastHandPosition = interactingHand.position;
        
        // Initialize the projected distance for this interaction
        // Use reference transform's up rather than our own
        float initialProj = Vector3.Dot(initialHandPositionRelative, referenceTransform.up) * sensitivity;
        lastProjectedDist = 0;  // Start with zero offset - we'll apply the offset in calculations
        
        if (debugMode)
        {
            Debug.Log($"Interaction started, initial hand projection: {initialProj:F4}");
        }
        
        // Play sound for feedback
        PlayInteractionSound();
    }
    
    // Handler for when interaction ends
    private void OnSelectExited(SelectExitEventArgs args)
    {
        isInteracting = false;
        interactingHand = null;
        
        // Animate to final value with a smooth transition
        AnimateToTargetOpacity();
        
        if (debugMode)
        {
            Debug.Log($"Interaction ended, final opacity: {currentOpacity:F2}");
        }
    }
    
    // Animate to the target opacity when interaction ends
    private void AnimateToTargetOpacity()
    {
        // Round to nearest 0.1 for a bit of snapping
        float roundedOpacity = Mathf.Round(currentOpacity * 10) / 10f;
        SetOpacityValue(roundedOpacity, true);
    }
    
    // Set opacity with option for animation
    private void SetOpacity(float opacity, bool animate)
    {
        opacity = Mathf.Clamp(opacity, minOpacity, maxOpacity);
        
        targetOpacity = opacity;
        
        if (animate)
        {
            // Start animation coroutine
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateOpacity(currentOpacity, targetOpacity));
        }
        else
        {
            // Apply immediately
            currentOpacity = targetOpacity;
            UpdateMaterialOpacity();
            UpdateButtonRotation();
        }
    }
    
    // Animation coroutine for smooth opacity transitions
    private IEnumerator AnimateOpacity(float startOpacity, float endOpacity)
    {
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            
            currentOpacity = Mathf.Lerp(startOpacity, endOpacity, t);
            UpdateMaterialOpacity();
            UpdateButtonRotation();
            
            yield return null;
        }
        
        // Ensure final state is exact
        currentOpacity = endOpacity;
        UpdateMaterialOpacity();
        UpdateButtonRotation();
        
        animationCoroutine = null;
    }
    
    // Update material opacity and related effects
    private void UpdateMaterialOpacity()
    {
        // 1. Update hologram opacity
        if (hologram != null)
        {
            Renderer objectRenderer = hologram.GetComponent<Renderer>();
            if (objectRenderer != null && materialIndex < objectRenderer.materials.Length)
            {
                Material mat = objectRenderer.materials[materialIndex];
                if (mat != null && mat.HasProperty(opacityPropertyName))
                {
                    mat.SetFloat(opacityPropertyName, currentOpacity);
                }
            }
        }
        
        // 2. Update emitter emission intensity
        if (hologramEmitter != null)
        {
            Renderer emitterRenderer = hologramEmitter.GetComponent<Renderer>();
            if (emitterRenderer != null && emitterRenderer.material != null)
            {
                Material emitterMat = emitterRenderer.material;
                if (emitterMat.HasProperty(emissionPropertyName))
                {
                    // Calculate emission intensity based on opacity
                    float emissionIntensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, currentOpacity);
                    
                    // Create a color with only green component
                    Color targetEmission = new Color(0, Mathf.Abs(emissionIntensity), 0, 1);
                    
                    // Set the emission color with new intensity
                    emitterMat.SetColor(emissionPropertyName, targetEmission);
                    
                    // Make sure emission is enabled if we're using it
                    if (emissionIntensity != 0)
                    {
                        emitterMat.EnableKeyword("_EMISSION");
                    }
                }
            }
        }
        
        // 3. Update light intensity
        if (hologramLight != null)
        {
            Light light = hologramLight.GetComponent<Light>();
            if (light != null)
            {
                // Scale light intensity based on opacity
                float lightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, currentOpacity);
                light.intensity = lightIntensity;
            }
        }
    }
    
    // Play sound feedback
    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    
    // Public API
    
    // Get current opacity
    public float GetCurrentOpacity()
    {
        return currentOpacity;
    }
    
    // Set opacity with animation option
    public void SetOpacityValue(float opacity, bool animate = true)
    {
        SetOpacity(opacity, animate);
    }
    
    // Set to minimum opacity
    public void SetMinimumOpacity(bool animate = true)
    {
        SetOpacity(minOpacity, animate);
    }
    
    // Set to maximum opacity
    public void SetMaximumOpacity(bool animate = true)
    {
        SetOpacity(maxOpacity, animate);
    }
    
    // Toggle debug mode - callable from inspector
    public void ToggleDebugMode()
    {
        debugMode = !debugMode;
        showDebugGizmos = debugMode;
        Debug.Log($"Debug mode: {(debugMode ? "ON" : "OFF")}");
    }
    
    // Visual debugging - show the reference transform's up axis
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw the reference transform's up axis in green (or our own if not assigned)
        Gizmos.color = Color.green;
        Vector3 referenceAxis = (referenceTransform != null) ? referenceTransform.up : transform.up;
        
        // Draw the control axis
        Gizmos.DrawLine(transform.position - referenceAxis * distanceScale/2, 
                        transform.position + referenceAxis * distanceScale/2);
        
        // Draw sphere for min, middle and max opacity positions
        Gizmos.color = new Color(1, 0, 0, 0.5f); // Min opacity
        Gizmos.DrawSphere(transform.position - referenceAxis * distanceScale/2, 0.005f);
        
        Gizmos.color = new Color(0, 1, 0, 0.5f); // Middle opacity
        Gizmos.DrawSphere(transform.position, 0.005f);
        
        Gizmos.color = new Color(0, 0, 1, 0.5f); // Max opacity
        Gizmos.DrawSphere(transform.position + referenceAxis * distanceScale/2, 0.005f);
        
        // Show rotation axis (X axis) in red
        Gizmos.color = new Color(1, 0.5f, 0.5f, 0.8f);
        Gizmos.DrawRay(transform.position, transform.right * 0.03f);
        Gizmos.DrawSphere(transform.position + transform.right * 0.03f, 0.003f);
    }
}
