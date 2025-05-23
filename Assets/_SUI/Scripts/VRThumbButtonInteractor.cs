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
    
    private string opacityPropertyName = "_Opacity";
    private string emissionPropertyName = "_EmissionColor";
    private int materialIndex = 0;
    private float minEmissionIntensity = 0f;
    private float maxEmissionIntensity = 1f;
    private float minLightIntensity = 0f;
    private float maxLightIntensity = 0.02f;
    
    private float currentOpacity = 1f;
    private float targetOpacity = 1f;
    private Coroutine animationCoroutine;
    
    private XRSimpleInteractable interactable;
    private Transform interactingHand;
    private bool isInteracting = false;
    
    private Vector3 initialHandPositionRelative;
    private Vector3 lastHandPosition;
    private float lastProjectedDist = 0f;
    private float projectionSmoothing = 0.2f; 
    
    private float lastAudioTime = 0f;
    private float audioDelay = 0.2f; 
    
    private bool debugMode = false;
    private int debugFrameCounter = 0;
    private const int DEBUG_LOG_INTERVAL = 60; 
    
    void Awake()
    {

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
            Debug.Log("Added XRSimpleInteractable component");
        }
        

        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.05f, 0.05f, 0.05f); 
            Debug.Log("Added BoxCollider component");
        }
        
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }
    
    void Start()
    {
        if (hologram == null)
        {
            hologram = gameObject;
            Debug.LogWarning("No hologram object assigned, using this gameObject instead.");
        }
        
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
        
        SetOpacityValue(maxOpacity, false);
    }
    
    void Update()
    {
        if (isInteracting && interactingHand != null)
        {
            CalculateOpacityFromHandPosition();
            
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
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
    }
    
    private void CalculateOpacityFromHandPosition()
    {
        if (interactingHand == null) return;
        
        Vector3 handPos = interactingHand.position;
        
        Vector3 buttonToHand = handPos - transform.position;
        
        Vector3 referenceAxis = referenceTransform.up;
        
        float rawProjectedDist = -Vector3.Dot(buttonToHand, referenceAxis) * sensitivity;
        
        float initialOffset = -Vector3.Dot(initialHandPositionRelative, referenceAxis) * sensitivity;
        
        float currentHandProjection = rawProjectedDist - initialOffset;
        
        float currentSmoothing = Mathf.Lerp(0.5f, 0.1f, Mathf.Abs(currentHandProjection - lastProjectedDist) * 10f);
        float smoothedProjectedDist = Mathf.Lerp(lastProjectedDist, currentHandProjection, 1 - currentSmoothing);
        
        lastProjectedDist = smoothedProjectedDist;
        
        if (showDebugGizmos)
        {
            Debug.DrawLine(transform.position, transform.position + referenceAxis * distanceScale, Color.green, 0.01f);
            Debug.DrawLine(transform.position, transform.position + referenceAxis * smoothedProjectedDist, Color.yellow, 0.01f);
        }
        
        float normalizedDistance = Mathf.InverseLerp(-distanceScale/2, distanceScale/2, smoothedProjectedDist);
        normalizedDistance = Mathf.Clamp01(normalizedDistance);

        float newOpacity = Mathf.Lerp(maxOpacity, minOpacity, normalizedDistance);
        
        if (Mathf.Abs(newOpacity - currentOpacity) > 0.01f)
        {
            SetOpacityValue(newOpacity, false);
            
            if (Mathf.Abs(newOpacity - currentOpacity) > 0.05f && Time.time - lastAudioTime > audioDelay)
            {
                PlayInteractionSound();
                lastAudioTime = Time.time;
            }
        }
        
        UpdateButtonRotation();
    }
    
    private void UpdateButtonRotation()
    {
        float normalizedOpacity = Mathf.InverseLerp(minOpacity, maxOpacity, currentOpacity);
        float rotationAngle = normalizedOpacity * knobRotationMax;
        transform.localRotation = Quaternion.Euler(rotationAngle, 0, 0);
    }
    
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isInteracting) return;
        
        isInteracting = true;
        interactingHand = args.interactorObject.transform;
        
        if (interactingHand == null)
        {
            Debug.LogError("Interaction started but hand reference is null!");
            isInteracting = false;
            return;
        }
        
        initialHandPositionRelative = interactingHand.position - transform.position;
        lastHandPosition = interactingHand.position;
        
        float initialProj = Vector3.Dot(initialHandPositionRelative, referenceTransform.up) * sensitivity;
        lastProjectedDist = 0; 
        
        if (debugMode)
        {
            Debug.Log($"Interaction started, initial hand projection: {initialProj:F4}");
        }
        
        PlayInteractionSound();
    }
    
    private void OnSelectExited(SelectExitEventArgs args)
    {
        isInteracting = false;
        interactingHand = null;
        
        AnimateToTargetOpacity();
        
        if (debugMode)
        {
            Debug.Log($"Interaction ended, final opacity: {currentOpacity:F2}");
        }
    }
    
    private void AnimateToTargetOpacity()
    {
        float roundedOpacity = Mathf.Round(currentOpacity * 10) / 10f;
        SetOpacityValue(roundedOpacity, true);
    }
    
    private void SetOpacity(float opacity, bool animate)
    {
        opacity = Mathf.Clamp(opacity, minOpacity, maxOpacity);
        
        targetOpacity = opacity;
        
        if (animate)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateOpacity(currentOpacity, targetOpacity));
        }
        else
        {
            currentOpacity = targetOpacity;
            UpdateMaterialOpacity();
            UpdateButtonRotation();
        }
    }
    
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
        
        currentOpacity = endOpacity;
        UpdateMaterialOpacity();
        UpdateButtonRotation();
        
        animationCoroutine = null;
    }
    
    private void UpdateMaterialOpacity()
    {
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
        
        if (hologramEmitter != null)
        {
            Renderer emitterRenderer = hologramEmitter.GetComponent<Renderer>();
            if (emitterRenderer != null && emitterRenderer.material != null)
            {
                Material emitterMat = emitterRenderer.material;
                if (emitterMat.HasProperty(emissionPropertyName))
                {
                    float emissionIntensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, currentOpacity);
                    
                    Color targetEmission = new Color(0, Mathf.Abs(emissionIntensity), 0, 1);
                    
                    emitterMat.SetColor(emissionPropertyName, targetEmission);
                    
                    if (emissionIntensity != 0)
                    {
                        emitterMat.EnableKeyword("_EMISSION");
                    }
                }
            }
        }
        
        if (hologramLight != null)
        {
            Light light = hologramLight.GetComponent<Light>();
            if (light != null)
            {
                float lightIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, currentOpacity);
                light.intensity = lightIntensity;
            }
        }
    }
    
    private void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
    }
    

    public float GetCurrentOpacity()
    {
        return currentOpacity;
    }
    
    public void SetOpacityValue(float opacity, bool animate = true)
    {
        SetOpacity(opacity, animate);
    }
    
    public void SetMinimumOpacity(bool animate = true)
    {
        SetOpacity(minOpacity, animate);
    }
    
    public void SetMaximumOpacity(bool animate = true)
    {
        SetOpacity(maxOpacity, animate);
    }
    
    public void ToggleDebugMode()
    {
        debugMode = !debugMode;
        showDebugGizmos = debugMode;
        Debug.Log($"Debug mode: {(debugMode ? "ON" : "OFF")}");
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = Color.green;
        Vector3 referenceAxis = (referenceTransform != null) ? referenceTransform.up : transform.up;
        
        Gizmos.DrawLine(transform.position - referenceAxis * distanceScale/2, 
                        transform.position + referenceAxis * distanceScale/2);
        
        Gizmos.color = new Color(1, 0, 0, 0.5f); 
        Gizmos.DrawSphere(transform.position - referenceAxis * distanceScale/2, 0.005f);
        
        Gizmos.color = new Color(0, 1, 0, 0.5f); 
        Gizmos.DrawSphere(transform.position, 0.005f);
        
        Gizmos.color = new Color(0, 0, 1, 0.5f); 
        Gizmos.DrawSphere(transform.position + referenceAxis * distanceScale/2, 0.005f);
        
        Gizmos.color = new Color(1, 0.5f, 0.5f, 0.8f);
        Gizmos.DrawRay(transform.position, transform.right * 0.03f);
        Gizmos.DrawSphere(transform.position + transform.right * 0.03f, 0.003f);
    }
}
