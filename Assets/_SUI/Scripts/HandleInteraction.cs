using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
                    
                    UpdateYearValue();
                    UpdateHandleRotation();
                    UpdateWire();
                }
                
                // For VR: Replace with controller grip/button release detection
                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    isDragging = false;
                    StartRetraction();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception during drag: {e.Message}");
                isDragging = false;
                StartRetraction();
            }
        }
    }
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
            
            // Apply a 90-degree rotation around the Y axis to correct the orientation
            targetRotation *= Quaternion.Euler(0, 90, 0);
            
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

    void UpdateYearValue()
    {
        // Calculate how far the handle is from its rest position, normalized from 0 to 1
        float distanceFromRest = Vector3.Distance(transform.position, restPosition);
        float maxExtensionDistance = 0.5f; // This should match the maxDistance in Update
        float normalizedDistance = Mathf.Clamp01(distanceFromRest / maxExtensionDistance);
        
        // Map the normalized distance to our year range
        float yearRange = maxYear - minYear;
        float yearValue = minYear + (yearRange * normalizedDistance);
        
        // Round to nearest step
        int roundedYear = Mathf.RoundToInt(yearValue / yearStep) * (int)yearStep;
        
        // Only update and print if the year has changed
        if (roundedYear != currentYear)
        {
            currentYear = roundedYear;
            PrintYearValue();
        }
    }
    
    void PrintYearValue()
    {
        string yearText;
        if (currentYear < 0)
        {
            yearText = $"{Mathf.Abs(currentYear)} BC";
        }
        else
        {
            yearText = $"{currentYear} AC";
        }
        
        Debug.Log($"Current Year: {yearText}");
        
        // Play the audio feedback when the year changes
        PlayYearChangeSound();
        
        // Here you can implement your UI update later
        // For example: yearTextDisplay.text = yearText;
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
