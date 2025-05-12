using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class ThumbButtonInteractor : ButtonInteractorBase
{
    [Header("Thumb Button Settings")]
    public float rotationSpeed = 2.0f;
    [Range(0f, 1f)]
    public float minOpacity = 0f;
    [Range(0f, 1f)]
    public float maxOpacity = 1f;    [HideInInspector]
    public string opacityPropertyName = "_Opacity";
    [HideInInspector]
    public string emissionPropertyName = "_EmissionColor";
    [HideInInspector]
    private int materialIndex = 0; // Keep private for internal use
      // Min and max values for hologram emitter emission intensity
    [HideInInspector]
    public float minEmissionIntensity = 0f;  // No emission when hologram is fully visible
    [HideInInspector]
    public float maxEmissionIntensity = 1f;  // Maximum green emission when hologram is invisible
    
    // Min and max values for hologram light intensity
    [HideInInspector]
    public float minLightIntensity = 0f;
    [HideInInspector]
    public float maxLightIntensity = 0.02f;
    
    [Header("References")]
    public Transform rotationPoint; // The point around which the cylinder rotates
    [Tooltip("The GameObject whose opacity will be controlled")]
    public GameObject hologram;
    public GameObject hologramEmitter;
    public GameObject hologramLight;
    
    [Header("UI References")]
    [Tooltip("TextMeshPro component to display the current selection")]
    public TextMeshPro displayText;
    public Color textHighlightColor = Color.yellow;
    public Color textNormalColor = Color.white;
    
    // Rotation settings
    private float minRotation = 0f;
    private float maxRotation = 180f;
    private float currentOpacity = 1f;
    
    // Interaction state
    private bool isDragging = false;
    private float dragStartY;
    private float startRotationX;
    private float targetRotation;
    private float currentRotationX;
      protected override void Start()
    {
        base.Start();
        
        // Initialize the cylinder
        if (rotationPoint == null)
        {
            rotationPoint = transform;
            Debug.LogWarning("No rotation point assigned, using this transform instead.");
        }
        
        // Initialize the objects if not assigned
        if (hologram == null)
        {
            hologram = gameObject;
            Debug.LogWarning("No hologram object assigned, using this gameObject instead.");
        }
        
        // No need to warn for optional emitter and light
        
        // Set initial rotation
        currentRotationX = transform.localRotation.eulerAngles.x;
        // Set initial opacity (use max at start)
        SetOpacity(maxOpacity, false);
        
        // Update text display
        UpdateDisplayText();
    }
    
    protected override void InitializeButton()
    {
        // Validate opacity range
        if (minOpacity > maxOpacity)
        {
            float temp = minOpacity;
            minOpacity = maxOpacity;
            maxOpacity = temp;
            Debug.LogWarning("Min opacity was greater than max opacity. Values have been swapped.");
        }
        
        // Check if the renderer and material are valid
        if (hologram != null)
        {
            Renderer objectRenderer = hologram.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                // Ensure material index is valid
                if (materialIndex >= objectRenderer.materials.Length)
                {
                    materialIndex = 0;
                    Debug.LogWarning($"Material index out of range for target object. Set to 0. Max index: {objectRenderer.materials.Length - 1}");
                }
            }
            else
            {
                Debug.LogWarning("Target object does not have a Renderer component. Opacity control won't work.");
            }
        }
        
        // Initialize with max opacity
        SetOpacity(maxOpacity, false);
    }
    
    protected override bool CheckRaycastHit(RaycastHit hit)
    {
        return hit.transform == transform;
    }
    
    protected override void OnLeftClickPressed(Vector2 mousePosition)
    {
        isDragging = true;
        dragStartY = mousePosition.y;
        startRotationX = currentRotationX;
        
        // Stop any ongoing animations (except press animation)
        if (animationCoroutine != null && !isPressed)
        {
            StopAnimation();
        }
    }
    
    protected override void OnLeftClickDrag(Vector2 mousePosition)
    {
        if (isDragging)
        {
            // Calculate screen delta from start position
            float dragDeltaY = mousePosition.y - dragStartY;
            
            // Convert screen movement to rotation (scaled by sensitivity)
            float rotationDelta = dragDeltaY * rotationSpeed;
            float newRotation = Mathf.Clamp(startRotationX + rotationDelta, minRotation, maxRotation);
            
            // Apply rotation
            currentRotationX = newRotation;
            transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
              // Calculate opacity based on rotation 
            // Map rotation (minRotation to maxRotation) to opacity (minOpacity to maxOpacity)
            // Note: Higher rotation = higher opacity
            float normalizedRotation = Mathf.InverseLerp(minRotation, maxRotation, currentRotationX);
            float newOpacity = Mathf.Lerp(minOpacity, maxOpacity, normalizedRotation);
            
            // If opacity changed significantly
            if (Mathf.Abs(newOpacity - currentOpacity) > 0.01f)
            {
                // Set the new opacity
                SetOpacity(newOpacity, false);
                
                // Play sound for feedback
                PlayInteractionSound();
                
                // Update display
                UpdateDisplayText();
            }
        }
    }
    
    protected override void OnLeftClickReleased(Vector2 mousePosition)
    {
        if (isDragging)
        {
            isDragging = false;            // Calculate final opacity
            float normalizedRotation = Mathf.InverseLerp(minRotation, maxRotation, currentRotationX);
            float finalOpacity = Mathf.Lerp(minOpacity, maxOpacity, normalizedRotation);
            
            // Animate to final value
            SetOpacity(finalOpacity, true);
        }
    }
    
    protected override void OnRightClickPressed(Vector2 mousePosition)
    {
        Debug.Log($"Right-clicked - Current opacity: {currentOpacity:F2} ({Mathf.RoundToInt(currentOpacity * 100)}%)");
    }
    
    // We don't need to override HandleInput anymore since the base class handles it
    
    void SetOpacity(float opacity, bool animate)
    {
        // Clamp opacity to valid range
        opacity = Mathf.Clamp(opacity, minOpacity, maxOpacity);
        
        // Check if opacity changed
        bool opacityChanged = Mathf.Abs(opacity - currentOpacity) > 0.001f;
        currentOpacity = opacity;
          // Calculate target rotation (map opacity to rotation)
        // Direct mapping: higher opacity = higher rotation value
        float normalizedOpacity = Mathf.InverseLerp(minOpacity, maxOpacity, opacity);
        targetRotation = Mathf.Lerp(minRotation, maxRotation, normalizedOpacity);
        
        if (animate)
        {
            StopAnimation();
            animationCoroutine = StartCoroutine(AnimateRotation());
        }
        else
        {
            // Immediately set rotation
            currentRotationX = targetRotation;
            transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
            
            // Immediately update material
            UpdateMaterialOpacity();
        }
        
        // Update UI if opacity changed
        if (opacityChanged)
        {
            PlayInteractionSound();
            
            // Update text display
            UpdateDisplayText();
        }
    }
    
    IEnumerator AnimateRotation()
    {
        // Use the base class's animation coroutine
        float startRotation = currentRotationX;
        float startOpacity = currentOpacity;
        float targetOpacity = currentOpacity; // We've already set this in SetOpacity
        
        return AnimateCoroutine(
            // Update action
            (t) => {
                currentRotationX = Mathf.Lerp(startRotation, targetRotation, t);
                transform.localRotation = Quaternion.Euler(currentRotationX, 0, 0);
                
                // Update opacity during animation too
                UpdateMaterialOpacity();
            }
        );
    }
      // Update the target material's opacity and related effects
    void UpdateMaterialOpacity()
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
                {                    // Calculate emission intensity based on opacity
                    // When opacity is max (1), emission is minEmissionIntensity (0)
                    // When opacity is min (0), emission is maxEmissionIntensity (-1)
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
                // When opacity is max (1), light is maxLightIntensity (0.02)
                // When opacity is min (0), light is minLightIntensity (0)
                float lightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, currentOpacity);
                light.intensity = lightIntensity;
            }
        }
    }
    
    // New method to update the TextMeshPro display
    void UpdateDisplayText()
    {
        if (displayText != null)
        {
            // Format as percentage with highlight color
            int opacityPercentage = Mathf.RoundToInt(currentOpacity * 100);
            displayText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(textHighlightColor)}>{opacityPercentage}%</color>";
        }
    }
    
    // Public methods to access and set the current opacity
    public float GetCurrentOpacity()
    {
        return currentOpacity;
    }
    
    // Programmatically set the opacity
    public void SetOpacityValue(float opacity, bool animate = true)
    {
        SetOpacity(opacity, animate);
    }
    
    // Set opacity to minimum (make fully transparent)
    public void SetMinimumOpacity(bool animate = true)
    {
        SetOpacity(minOpacity, animate);
    }
    
    // Set opacity to maximum (make fully visible)
    public void SetMaximumOpacity(bool animate = true)
    {
        SetOpacity(maxOpacity, animate);
    }
}
