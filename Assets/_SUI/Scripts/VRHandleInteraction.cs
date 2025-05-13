using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// VR-compatible version of the HandleInteraction script designed to work with XRGrabInteractable.
/// This script handles the timeline manipulation functionality without conflicting with VR grab interactions.
/// 
/// IMPLEMENTATION NOTES:
/// - This script minimizes inspector-configurable values to reduce complexity
/// - Only crucial parameters that need tuning should be exposed in the inspector
/// - Debug options can be enabled temporarily during development
/// - Parameters like rotationCorrection are hardcoded to the correct values after testing
/// </summary>
public class VRHandleInteraction : MonoBehaviour
{    
    [Header("Interaction Points")]
    [Tooltip("The point where the controller attaches when grabbing")]
    public Transform grabPoint;
    [Tooltip("The anchor point that the handle extends from")]
    public Transform zeroPoint;
    [Tooltip("Wire renderer connecting the zero point to the handle")]
    public LineRenderer wireRenderer;
    
    [Header("Interaction Settings")]
    [Tooltip("Check this box to disable tracking position and letting XRGrabInteractable handle position")]
    public bool disablePositionTracking = false;
    [Tooltip("If true, the grabPoint will be shown in the editor")]
    public bool showGrabPointGizmo = true;
    // VR Interaction state tracking
    private bool isGrabbed = false;
    private Vector3 initialGrabPosition;
    private Vector3 originalRestPosition; // The original position where the handle should return to
    private Vector3 originalRestLocalPosition; // The original local position for the handle
    private Vector3 restPosition; // The current position where the handle should return to
    private Vector3 restLocalPosition; // The current local position where the handle should return
    private Quaternion initialRotation; // Store the initial rotation
    private Quaternion originalRestLocalRotation; // The original local rotation to return to
    private Quaternion restLocalRotation; // Current local rotation
    // Retraction settings
    public float retractSpeed = 5.0f; // Speed of retraction
    private bool isRetracting = false;
    
    [Header("Year Settings")]
    [Tooltip("Minimum year value (negative for BC)")]
    public float minYear = -2000f; // 2000 BC
    [Tooltip("Maximum year value (positive for AC)")]
    public float maxYear = 2000f;  // 2000 AC
    [Tooltip("Increment step for year snapping")]
    public float yearStep = 100f;   // Increment by 100 years
    
    [Header("Handle Extension")]
    [Tooltip("Maximum distance the handle can be pulled out - increase if handle movement isn't capturing full year range")]
    public float maxExtensionDistance = 1.2f; // Set to match actual VR movement distance
    
    // Debug options - not normally exposed
    private bool enableDebugLogs = false; // Set to false by default in production
    private int currentYear = 0;    // Current year value
    private float exactYear = 0f;   // Store the exact (non-rounded) year
    
    // Audio settings
    private AudioSource audioSource;
    public AudioClip yearChangeSound; // Assign this in the inspector    
    
    [Header("Number Cylinder References")]
    [Tooltip("References to the five number cylinder transforms that will rotate")]
    // References to the rotating number cylinders
    public Transform numberRotate1; // Units
    public Transform numberRotate2; // Tens
    public Transform numberRotate3; // Hundreds
    public Transform numberRotate4; // Thousands
    public Transform numberRotate5; // BC/AC indicator
    
    // Track previous rotations for smooth interpolation
    private Quaternion[] previousRotations = new Quaternion[5];
    public float rotationSmoothTime = 0.3f; // Time to smoothly rotate between numbers
    
    // Fixed rotation values - hardcoded for consistency
    private readonly float degreesPerDigit = 36f; // 36 degrees per digit (10 digits in 360 degrees)
    
    // Rotation correction for the handle when grabbed - fixed value based on testing
    private readonly Vector3 rotationCorrection = new Vector3(-90, 0, 0);
    
    [Header("Number Cylinder References")]
    [Tooltip("Reference check - make sure all number cylinders are assigned")]
    public bool checkNumberCylinders = true;
    
    // Private cache for needed components
    private XRGrabInteractable grabInteractable;
    
    // Dictionary to track active rotation coroutines
    private Dictionary<int, Coroutine> activeCoroutines = new Dictionary<int, Coroutine>();
    
    void Awake()
    {
        // Get XR components reference
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogError("VRHandleInteraction requires an XRGrabInteractable component!");
        }
        else
        {
            // Subscribe to XR grab/release events with properly typed methods
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
            
            // Set the attach transform to use the grabPoint
            if (grabPoint != null)
            {
                grabInteractable.attachTransform = grabPoint;
                Debug.Log("XRGrabInteractable attach transform set to grabPoint");
            }
            else
            {
                Debug.LogWarning("grabPoint is null! Object will grab at pivot point.");
            }
        }
    }    
    
    // Check for essential components and references
    void Start()
    {
        // Check for missing references
        if (grabPoint == null)
        {
            Debug.LogError("VRHandleInteraction: grabPoint is not assigned!");
        }
        
        if (zeroPoint == null)
        {
            Debug.LogError("VRHandleInteraction: zeroPoint is not assigned!");
        }
        
        if (wireRenderer == null)
        {
            Debug.LogError("VRHandleInteraction: wireRenderer is not assigned!");
        }
        
        // Check number cylinder references
        if (numberRotate1 == null) Debug.LogError("VRHandleInteraction: numberRotate1 is not assigned!");
        if (numberRotate2 == null) Debug.LogError("VRHandleInteraction: numberRotate2 is not assigned!");
        if (numberRotate3 == null) Debug.LogError("VRHandleInteraction: numberRotate3 is not assigned!");
        if (numberRotate4 == null) Debug.LogError("VRHandleInteraction: numberRotate4 is not assigned!");
        if (numberRotate5 == null) Debug.LogError("VRHandleInteraction: numberRotate5 is not assigned!");
        
        // Store both world and local positions/rotations as starting references
        originalRestPosition = transform.position;
        originalRestLocalPosition = transform.localPosition;
        originalRestLocalRotation = transform.localRotation;
        
        // Set current working positions to match originals
        restPosition = originalRestPosition;
        restLocalPosition = originalRestLocalPosition;
        restLocalRotation = originalRestLocalRotation;
        initialRotation = transform.rotation;

        // Get the audio source from the zero point
        if (zeroPoint != null)
        {
            audioSource = zeroPoint.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found on Pull_Zero_Point");
            }
        }

        // Initialize previous rotations
        for (int i = 0; i < previousRotations.Length; i++) 
        {
            previousRotations[i] = Quaternion.identity;
        }
        
        // Initialize number cylinders to starting position - use false for immediate positioning
        UpdateNumberRotation(currentYear, false);
        
        // Initialize the wire
        UpdateWire();
    }      
    
    void Update()
    {
        if (isGrabbed && grabInteractable != null && grabInteractable.isSelected)
        {
            // Update handle values based on position while grabbed
            UpdateHandleRotation();
            UpdateWire();
            
            // Add explicit call to ensure it runs
            UpdateExactYearValue();
            
            // Log occasionally to verify updates are happening
            if (enableDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[VRHandleInteraction] Handle is grabbed and updating - isGrabbed: {isGrabbed}, isSelected: {grabInteractable.isSelected}");
            }
        }
        else if (isRetracting && !grabInteractable.isSelected)
        {
            // Handle smooth retraction in Update rather than coroutine
            // This approach is more compatible with XRGrabInteractable
            if (Vector3.Distance(transform.localPosition, originalRestLocalPosition) > 0.01f)
            {
                // Smoothly move toward original rest position using local transforms for better VR compatibility
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalRestLocalPosition, Time.deltaTime * retractSpeed);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRestLocalRotation, Time.deltaTime * retractSpeed);
                
                // Update the wire as we move
                UpdateWire();
            }
            else
            {
                // When we're close enough, snap to final position
                transform.localPosition = originalRestLocalPosition;
                transform.localRotation = originalRestLocalRotation;
                UpdateWire();
                isRetracting = false;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (grabInteractable != null)
        {
            // Remove listeners with properly typed methods
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
    
    // Called when the object is grabbed in VR
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        
        // Store position at grab time for relative movement calculations
        initialGrabPosition = transform.position;
        
        // Double check that the attach transform is set correctly
        if (grabInteractable != null && grabPoint != null && grabInteractable.attachTransform != grabPoint)
        {
            Debug.LogWarning("Fixing attach transform during grab");
            grabInteractable.attachTransform = grabPoint;
        }
        
        // If we were retracting, stop that process
        if (isRetracting)
        {
            StopAllCoroutines();
            isRetracting = false;
        }
        
        // Reset the year value to start fresh from the grab position
        // This makes it more responsive to small movements
        exactYear = currentYear;
        
        // Force an immediate update of the year when grabbed
        UpdateWire();
        UpdateHandleRotation();
        UpdateExactYearValue();
        
        // Enable debug logs temporarily when grabbed to help troubleshoot
        enableDebugLogs = true;
        Debug.Log($"[VRHandleInteraction] Handle grabbed at position: {initialGrabPosition}");
    }
    
    // Called when the object is released in VR
    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("[VRHandleInteraction] Handle released");
        isGrabbed = false;
        
        // Snap to the nearest year step when released
        SnapToNearestStep();
        
        // Mark for returning to rest position in Update instead of using a coroutine
        isRetracting = true;
        
        // Disable extra debug logs
        enableDebugLogs = false;
    }
    
    void UpdateHandleRotation()
    {
        // Only manipulate rotation if position tracking isn't disabled
        if (disablePositionTracking)
            return;
            
        // Calculate direction from handle to zero point
        Vector3 direction = zeroPoint.position - transform.position;
        
        // Only update rotation if we have a valid direction
        if (direction.magnitude > 0.001f)
        {            
            // Create a rotation that points the handle's forward direction toward the zero point
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Apply rotation correction using fixed values
            targetRotation *= Quaternion.Euler(rotationCorrection);
            
            // Apply the rotation
            transform.rotation = targetRotation;
            
            // Log rotation occasionally for debugging
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"Handle rotation set to: {transform.rotation.eulerAngles}");
            }
        }
    }
    
    // This method is kept for compatibility but will no longer be used for VR interaction
    void StartRetraction()
    {
        if (!isRetracting)
        {
            isRetracting = true;
            // Don't use the coroutine approach as it conflicts with XRGrabInteractable
            // RetractHandle now happens in Update()
        }
    }

    // This coroutine is no longer used in VR mode
    IEnumerator RetractHandle()
    {
        // This is retained for backward compatibility but not used in VR
        yield break;
    }

    void UpdateWire()
    {
        // Simple direct line from zero point to handle
        wireRenderer.positionCount = 2;
        wireRenderer.SetPosition(0, zeroPoint.position);
        wireRenderer.SetPosition(1, transform.position);
    }    
    
    // Updates the year value based on handle position without rounding
    void UpdateExactYearValue()
    {
        // Simply calculate direct distance from zeroPoint to the handle's current position
        // This allows movement in any direction from the zero point
        float distanceFromZero = Vector3.Distance(transform.position, zeroPoint.position);
        
        // The problem is in this calculation - the initial offset might be too large
        // or there might be an issue with how we're determining the starting point
        
        // Instead of using the originalRestPosition, use the initialGrabPosition when grabbed
        // This creates a relative movement from the grab position
        float initialOffset = isGrabbed ? 
            Vector3.Distance(initialGrabPosition, zeroPoint.position) : 
            Vector3.Distance(originalRestPosition, zeroPoint.position);
        
        // Add a smaller threshold to make it more sensitive to small movements
        float effectiveDistance = Mathf.Max(0, distanceFromZero - initialOffset);
        
        // Add a sensitivity multiplier to make small movements more pronounced
        float sensitivityMultiplier = 2.0f;
        effectiveDistance *= sensitivityMultiplier;
        
        // Normalize the distance based on maximum extension
        float normalizedDistance = Mathf.Clamp01(effectiveDistance / maxExtensionDistance);
        
        // More frequent debug logging during interaction to help troubleshoot
        if (enableDebugLogs && Time.frameCount % 30 == 0) // Increased frequency for better debugging
        {
            Debug.Log($"[VRHandleInteraction] Distance: {distanceFromZero:F3}, Initial: {initialOffset:F3}, Effective: {effectiveDistance:F3}/{maxExtensionDistance:F3} = {normalizedDistance:F3}");
            Debug.Log($"[VRHandleInteraction] Handle Position: {transform.position}, Zero Point: {zeroPoint.position}, Initial Grab: {initialGrabPosition}");
        }

        // Map the normalized distance to our year range - no rounding
        float yearRange = maxYear - minYear;
        exactYear = minYear + (yearRange * normalizedDistance);
        
        // Debug the calculated year with more frequent logging
        if (enableDebugLogs && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[VRHandleInteraction] Year value: {exactYear:F1}, Normalized distance: {normalizedDistance:F3}");
        }
        
        // Update cylinder display with exact year (continuous movement)
        UpdateNumberRotation(0, exactYear, true);
    }
    
    // Snap to nearest step when releasing the handle
    void SnapToNearestStep()
    {
        // Round to nearest step
        int roundedYear = Mathf.RoundToInt(exactYear / yearStep) * (int)yearStep;
        
        // Record the current exact year for smooth transition
        float fromExactYear = exactYear;
        
        // Update current year for the system
        if (roundedYear != currentYear)
        {
            currentYear = roundedYear;
            
            // Log the new year
            string yearText;
            if (currentYear < 0)
            {
                yearText = $"{Mathf.Abs(currentYear)} BC";
            }
            else
            {
                yearText = $"{currentYear} AC";
            }
            Debug.Log($"Year set to: {yearText}");
            
            // Play the audio feedback when the year changes
            PlayYearChangeSound();
        }
        
        // Start a coroutine for smooth transition to the snapped year
        StartCoroutine(SmoothSnapToYear(fromExactYear, (float)currentYear));
    }
    
    // Coroutine for smooth transition between exact and snapped year
    IEnumerator SmoothSnapToYear(float fromYear, float toYear)
    {
        float snapDuration = 0.3f; // Duration of snapping animation
        float elapsedTime = 0f;
        
        while (elapsedTime < snapDuration)
        {
            // Calculate interpolated year based on cubic ease-out
            float t = elapsedTime / snapDuration;
            t = 1 - Mathf.Pow(1 - t, 3); // Same easing as the handle retraction
            
            float interpolatedYear = Mathf.Lerp(fromYear, toYear, t);
            
            // Update the cylinder rotations with the interpolated year
            UpdateNumberRotation(0, interpolatedYear, false);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state is exact
        UpdateNumberRotation(0, toYear, false);
    }
    
    // Update the overloaded methods to handle the new workflow
    void UpdateNumberRotation(int year, float exactYearValue, bool smoothTransition)
    {
        // Get absolute year value for digit extraction
        float absExactYear = Mathf.Abs(exactYearValue);
        
        // Extract fractional parts for all digits for smoother rotation
        float thousandsFloat = (absExactYear / 1000) % 10;
        float hundredsFloat = (absExactYear / 100) % 10;
        float tensFloat = (absExactYear / 10) % 10;
        float unitsFloat = absExactYear % 10;
        
        // Debug year digit extraction
        if (enableDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[VRHandleInteraction] Year {exactYearValue:F1} -> Digits: {thousandsFloat:F2} {hundredsFloat:F2} {tensFloat:F2} {unitsFloat:F2}");
        }
        
        // Calculate target rotations using floating point values for smoother transitions
        Quaternion[] targetRotations = new Quaternion[5];
        targetRotations[0] = Quaternion.Euler(thousandsFloat * degreesPerDigit, 0, 0); // Use fixed degreesPerDigit value
        targetRotations[1] = Quaternion.Euler(hundredsFloat * degreesPerDigit, 0, 0);
        targetRotations[2] = Quaternion.Euler(tensFloat * degreesPerDigit, 0, 0);
        targetRotations[3] = Quaternion.Euler(unitsFloat * degreesPerDigit, 0, 0);
        
        // Set BC/AC cylinder (BC at 0 degrees, AC at 180 degrees)
        float bcacRotation = (exactYearValue < 0) ? 0f : 180f;
        targetRotations[4] = Quaternion.Euler(bcacRotation, 0, 0);
        
        // Apply rotations to cylinders
        Transform[] cylinders = { numberRotate1, numberRotate2, numberRotate3, numberRotate4, numberRotate5 };
        
        // Debug log to verify cylinders are assigned
        if (enableDebugLogs && Time.frameCount % 300 == 0) // Only check occasionally
        {
            string cylinderStatus = "[VRHandleInteraction] Number cylinders status: ";
            for (int i = 0; i < cylinders.Length; i++) {
                cylinderStatus += cylinders[i] == null ? "NULL " : "OK ";
            }
            Debug.Log(cylinderStatus);
            
            // Log the current rotations to verify they're changing
            if (cylinders[0] != null)
            {
                Debug.Log($"[VRHandleInteraction] First cylinder rotation: {cylinders[0].localRotation.eulerAngles}");
            }
        }
        
        for (int i = 0; i < cylinders.Length; i++) {
            if (cylinders[i] != null) {
                if (smoothTransition) {
                    // Start a coroutine for smooth rotation
                    StopCoroutineForCylinder(i);
                    StartCoroutine(SmoothRotate(cylinders[i], targetRotations[i], i));
                } else {
                    // Apply rotation immediately
                    cylinders[i].localRotation = targetRotations[i];
                    previousRotations[i] = targetRotations[i];
                }
                
                // Debug the applied rotations occasionally
                if (enableDebugLogs && Time.frameCount % 180 == 0 && i == 0) {
                    Debug.Log($"[VRHandleInteraction] Cylinder {i} rotation: Target={targetRotations[i].eulerAngles}, Actual={cylinders[i].localRotation.eulerAngles}");
                }
            }
        }
    }
    
    // Original method to maintain compatibility with other calls
    void UpdateNumberRotation(int year, bool smoothTransition)
    {
        UpdateNumberRotation(year, (float)year, smoothTransition);
    }
    
    // Stop any existing coroutine for a cylinder
    void StopCoroutineForCylinder(int index) {
        if (activeCoroutines.TryGetValue(index, out Coroutine routine)) {
            if (routine != null) {
                StopCoroutine(routine);
            }
            activeCoroutines.Remove(index);
        }
    }
    
    // Coroutine for smooth rotation
    IEnumerator SmoothRotate(Transform cylinder, Quaternion targetRotation, int index) {
        float elapsedTime = 0;
        Quaternion startRotation = cylinder.localRotation;
        
        // Debug rotation start
        if (enableDebugLogs && index == 0) {
            Debug.Log($"[VRHandleInteraction] Starting smooth rotation for cylinder {index}: {startRotation.eulerAngles} -> {targetRotation.eulerAngles}");
        }
        
        while (elapsedTime < rotationSmoothTime) {
            cylinder.localRotation = Quaternion.Slerp(
                startRotation, 
                targetRotation, 
                elapsedTime / rotationSmoothTime
            );
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we land exactly on the target rotation
        cylinder.localRotation = targetRotation;
        previousRotations[index] = targetRotation;
        
        // Debug rotation completion
        if (enableDebugLogs && index == 0) {
            Debug.Log($"[VRHandleInteraction] Completed smooth rotation for cylinder {index}: Final rotation = {cylinder.localRotation.eulerAngles}");
        }
        
        // Remove from active coroutines
        activeCoroutines.Remove(index);
    }
    
    void PlayYearChangeSound()
    {
        if (audioSource != null && audioSource.isActiveAndEnabled)
        {
            // Use PlayOneShot for better sound handling
            if (yearChangeSound != null)
            {
                audioSource.PlayOneShot(yearChangeSound);
            }
            else
            {
                // Fallback to Play() if no clip is assigned
                audioSource.Play();
            }
        }
    }    
    
    // No need for custom event classes as we're using the built-in XR Interaction Toolkit events
    
    // Draw gizmos to visualize the grab point in the editor
    void OnDrawGizmos()
    {
        if (showGrabPointGizmo && grabPoint != null)
        {
            // Draw a sphere at the grab point location
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(grabPoint.position, 0.02f);
            
            // Draw a line from the object's pivot to the grab point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, grabPoint.position);
        }
    }
}
