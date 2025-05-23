using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class HandleInteraction : MonoBehaviour
{
    public Transform grabPoint;
    public Transform zeroPoint;
    public LineRenderer wireRenderer;
    // VR Interaction: When implementing VR support, replace the following mouse input variables 
    // with appropriate VR controller references and interaction states
    private bool isDragging = false;
    private Vector2 initialMousePosition;
    private Vector3 initialHandlePosition;
    private Vector3 restPosition; // The position where the handle rests when not being dragged
    public float retractSpeed = 5.0f; // Speed of retraction
    private bool isRetracting = false;
    private Quaternion initialRotation; // Store the initial rotation

    public float minYear = -2000f; // 2000 BC
    public float maxYear = 2000f;  // 2000 AC
    public float yearStep = 100f;   // Increment by 100 years
    private int currentYear = 0;    // Current year value

    private AudioSource audioSource;
    public AudioClip yearChangeSound; // Assign this in the inspector

    // References to the rotating number cylinders
    public Transform numberRotate1; // Units
    public Transform numberRotate2; // Tens
    public Transform numberRotate3; // Hundreds
    public Transform numberRotate4; // Thousands
    public Transform numberRotate5; // BC/AC indicator

    // Track previous rotations for smooth interpolation
    private Quaternion[] previousRotations = new Quaternion[5];
    public float rotationSmoothTime = 0.3f; // Time to smoothly rotate between numbers

    private float exactYear = 0f; // Store the exact (non-rounded) year

    void Start()
    {
        // Check for missing references
        if (grabPoint == null)
        {
            Debug.LogError("HandleInteraction: grabPoint is not assigned!");
        }
        
        if (zeroPoint == null)
        {
            Debug.LogError("HandleInteraction: zeroPoint is not assigned!");
        }
        
        if (wireRenderer == null)
        {
            Debug.LogError("HandleInteraction: wireRenderer is not assigned!");
        }
        
        // Store the initial rest position and rotation
        restPosition = transform.position;
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
        for (int i = 0; i < previousRotations.Length; i++) {
            previousRotations[i] = Quaternion.identity;
        }
        
        // Initialize number cylinders to starting position - use false for immediate positioning
        UpdateNumberRotation(currentYear, false);
    }

    void Update()
    {
        // Handle all input via this method (replace with VR input handling in the future)
        HandleInput();
    }

    // ===== VR INTERACTION REPLACEMENT START =====
    // This method handles mouse input detection and should be replaced with VR controller input
    // when implementing VR support. Replace these mouse-specific calls with VR equivalents.
    void HandleInput()
    {
        // Check for input device
        if (Mouse.current == null) return;
        Mouse mouse = Mouse.current;
        
        // Check for null references
        if (grabPoint == null || zeroPoint == null || wireRenderer == null) return;

        // Handle initiation of grab/interaction
        if (mouse.leftButton.wasPressedThisFrame)
        {
            try
            {
                Vector2 mousePosition = mouse.position.ReadValue();
                if (Camera.main == null) return;
                
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                // For VR: Replace with controller raycast or direct collision detection
                if (Physics.Raycast(ray, out hit) && hit.transform == grabPoint)
                {
                    isDragging = true;
                    initialMousePosition = mousePosition;
                    initialHandlePosition = transform.position;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during handle interaction: {e.Message}");
            }
        }

        // Handle active dragging/movement of the handle
        if (isDragging)
        {
            try
            {
                // For VR: Replace mousePosition with VR controller position
                Vector2 mousePosition = mouse.position.ReadValue();
                
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                
                // For VR: Instead of creating a plane relative to the camera,
                // you would likely use the direct controller position/movement
                Plane movementPlane = new Plane(-mainCamera.transform.forward, initialHandlePosition);
                
                // For VR: Replace with controller position tracking
                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                
                if (movementPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    
                    // This part can remain similar in VR, but would use controller position
                    // instead of raycast hit point
                    Vector3 newHandlePosition = hitPoint;
                    
                    // Limit distance - keep this for VR as well
                    Vector3 directionToZero = newHandlePosition - zeroPoint.position;
                    float maxDistance = 0.5f; // Adjust this value as needed
                    if (directionToZero.magnitude > maxDistance)
                    {
                        directionToZero = directionToZero.normalized * maxDistance;
                        newHandlePosition = zeroPoint.position + directionToZero;
                    }
                    
                    transform.position = newHandlePosition;
                    
                    // Update year based on exact position (without rounding)
                    UpdateExactYearValue();
                    
                    UpdateHandleRotation();
                    UpdateWire();
                }
                
                // When releasing the handle, snap to nearest 100s
                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    isDragging = false;
                    SnapToNearestStep();
                    StartRetraction();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during drag: {e.Message}");
                isDragging = false;
                StartRetraction();
            }
        }    }
    
    // ===== VR INTERACTION REPLACEMENT END =====
    
    void UpdateHandleRotation()
    {
        // Calculate direction from handle to zero point
        Vector3 direction = zeroPoint.position - transform.position;
        
        // Only update rotation if we have a valid direction
        if (direction.magnitude > 0.001f)
        {
            // Create a rotation that points the handle's forward direction toward the zero point
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Apply rotation correction - try Z-axis rotation instead
            targetRotation *= Quaternion.Euler(-90, 0, 0);
            
            // Apply the rotation
            transform.rotation = targetRotation;
        }
    }

    void StartRetraction()
    {
        if (!isRetracting)
        {
            isRetracting = true;
            StartCoroutine(RetractHandle());
        }
    }

    IEnumerator RetractHandle()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float journeyLength = Vector3.Distance(startPosition, restPosition);
        float startTime = Time.time;
        
        // Continue until we're very close to the rest position
        while (Vector3.Distance(transform.position, restPosition) > 0.01f)
        {
            // Calculate how far along the journey we are (0 to 1)
            float distCovered = (Time.time - startTime) * retractSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            
            // Use a smooth easing function for natural spring-like movement
            float t = 1 - Mathf.Pow(1 - fractionOfJourney, 3); // Cubic ease-out
            
            // Set our position
            transform.position = Vector3.Lerp(startPosition, restPosition, t);
            
            // Smoothly interpolate rotation from current to initial rotation
            transform.rotation = Quaternion.Slerp(startRotation, initialRotation, t);
            
            // Update the wire
            UpdateWire();
            
            yield return null;
        }
        
        // Ensure we end exactly at the rest position and rotation
        transform.position = restPosition;
        transform.rotation = initialRotation;
        UpdateWire();
        isRetracting = false;
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
        // Calculate how far the handle is from its rest position, normalized from 0 to 1
        float distanceFromRest = Vector3.Distance(transform.position, restPosition);
        float maxExtensionDistance = 0.5f; // This should match the maxDistance in HandleInput
        float normalizedDistance = Mathf.Clamp01(distanceFromRest / maxExtensionDistance);
        
        // Map the normalized distance to our year range - no rounding
        float yearRange = maxYear - minYear;
        exactYear = minYear + (yearRange * normalizedDistance);
        
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

    // Remove or modify the unused methods
    void UpdateYearValue()
    {
        // This method is no longer needed but kept for compatibility
        // It will be replaced by UpdateExactYearValue and SnapToNearestStep
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
        
        // Calculate target rotations using floating point values for smoother transitions
        Quaternion[] targetRotations = new Quaternion[5];
        targetRotations[0] = Quaternion.Euler(thousandsFloat * 30f, 0, 0);
        targetRotations[1] = Quaternion.Euler(hundredsFloat * 30f, 0, 0);
        targetRotations[2] = Quaternion.Euler(tensFloat * 30f, 0, 0);
        targetRotations[3] = Quaternion.Euler(unitsFloat * 30f, 0, 0);
        
        // Set BC/AC cylinder (BC at 0 degrees, AC at 180 degrees)
        float bcacRotation = (exactYearValue < 0) ? 0f : 180f;
        targetRotations[4] = Quaternion.Euler(bcacRotation, 0, 0);
        
        // Apply rotations to cylinders
        Transform[] cylinders = { numberRotate1, numberRotate2, numberRotate3, numberRotate4, numberRotate5 };
        
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
            }
        }
    }
    
    // Original method to maintain compatibility with other calls
    void UpdateNumberRotation(int year, bool smoothTransition)
    {
        UpdateNumberRotation(year, (float)year, smoothTransition);
    }
    
    // Dictionary to track active rotation coroutines
    private Dictionary<int, Coroutine> activeCoroutines = new Dictionary<int, Coroutine>();
    
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
}
