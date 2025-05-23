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
    [SerializeField] private InteractionMode interactionMode = InteractionMode.AllMethods; 

    public enum InteractionMode
    {
        PokeTrigger, 
        HoverTrigger,  
        SelectTrigger,  
        AllMethods     
    }

    private Vector3 originalPosition;
    private bool isPressed = false;
    private Coroutine animationCoroutine;
    private bool isScreenOn = false;
    private Renderer screenRenderer;
    private bool isHovering = false;
    private float lastToggleTime = 0f;
    [SerializeField] private float toggleCooldown = 0.2f; 

    private void Awake()
    {
        originalPosition = transform.localPosition;
        
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
        
        UpdateScreenState();
    }

    private void Start()
    {
        Debug.Log("VR Button Interactor initialized");
        
        if (!TryGetComponent<XRSimpleInteractable>(out var simpleInteractable))
        {
            Debug.LogError("Missing XRSimpleInteractable component. Please add one to this GameObject.");
        }
        else
        {
            if (simpleInteractable.selectEntered.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("<color=yellow>No select events registered in the Inspector. Adding programmatically.</color>");
                
                simpleInteractable.selectEntered.AddListener(OnButtonInteraction);
            }
        }
        
        if (showPokeZone)
        {
            CreatePokeVisualizer();
        }
    }

    private void OnValidate()
    {
        Debug.Log("VR Button Setup Guide:\n" +
                  "1. Adjust XR Poke Interactor on your controller:\n" +
                  "   - Reduce Poke Depth to 0.05-0.07m (currently " + 0.1f + "m)\n" +
                  "   - Reduce Poke Width to 0.005m (currently " + 0.0075f + "m)\n" +
                  "   - Reduce Poke Select Width to 0.01m (currently " + 0.015f + "m)\n" +
                  "   - Reduce Poke Hover Radius to 0.01m (currently " + 0.015f + "m)\n" +
                  "   - Reduce Poke Interaction Offset to 0.002m (currently " + 0.005f + "m)\n" +
                  "2. Make sure the Mesh Collider size closely matches the visual button");
    }



    /// <summary>
    /// </summary>
    public void ToggleScreen()
    {
        if (Time.time - lastToggleTime < toggleCooldown)
            return;
        
        lastToggleTime = Time.time;
        
        Debug.Log("ToggleScreen called directly");
        
        isScreenOn = !isScreenOn;
        
        UpdateScreenState();
        

        PlayInteractionSound();
        

        PressAndReleaseAnimation();
        
        Debug.Log($"Screen toggled to: {(isScreenOn ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// </summary>
    public void OnHoverEntered(HoverEnterEventArgs args = null)
    {
        if (detailedDebugLogging && args != null)
        {
            Debug.Log($"<color=yellow>HOVER ENTER from: {args.interactorObject.transform.name}</color>");
            Debug.Log($"<color=yellow>Interactor type: {args.interactorObject.GetType().Name}</color>");
        }
        
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
            isHovering = true;
        }
    }
    
    /// <summary>
    /// </summary>
    public void OnHoverExited() 
    {
        isHovering = false;
        ReleaseButton(); 
    }
    
    /// <summary>
    /// </summary>
    public void OnButtonInteraction(SelectEnterEventArgs args = null)
    {
        if (detailedDebugLogging && args != null)
        {
            Debug.Log($"<color=green>SELECT ENTER from: {args.interactorObject.transform.name}</color>");
            Debug.Log($"<color=green>Interactor type: {args.interactorObject.GetType().Name}</color>");
            
            bool isNearFarInteractor = args.interactorObject.transform.name.Contains("Near-Far");
            Debug.Log($"<color=cyan>Is NearFarInteractor: {isNearFarInteractor}</color>");
        }
        
        if (interactionMode == InteractionMode.SelectTrigger || 
            interactionMode == InteractionMode.AllMethods ||
            interactionMode == InteractionMode.PokeTrigger) 
        {
            ToggleScreen();
        }
    }

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


    private void UpdateScreenState()
    {
        if (screenObject == null)
            return;
            
        if (isScreenOn)
        {

            screenObject.SetActive(true);
            

            if (screenRenderer != null && screenOnMaterial != null)
            {
                screenRenderer.material = screenOnMaterial;
            }
        }
        else
        {

            if (screenRenderer != null && screenOffMaterial != null)
            {
                screenRenderer.material = screenOffMaterial;
            }
            else
            {

                screenObject.SetActive(false);
            }
        }
    }

    private void PressAndReleaseAnimation()
    {

        AnimateButtonPress(true);
        

        Invoke(nameof(ReleaseButton), 0.1f);
    }
    
    private void ReleaseButton()
    {
        AnimateButtonPress(false);
    }


    private void AnimateButtonPress(bool press)
    {
        isPressed = press;
        

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        

        animationCoroutine = StartCoroutine(AnimateButtonPressCoroutine(press));
    }
    
    private IEnumerator AnimateButtonPressCoroutine(bool press)
    {
        float elapsed = 0f;
        

        Vector3 startPosition = transform.localPosition;
        
        Vector3 pressVector = Vector3.zero;
        switch (pressAxis)
        {
            case PressAxis.Y:
                pressVector = Vector3.down * pressDepth;  
                break;
            case PressAxis.Z:
                pressVector = Vector3.back * pressDepth;
                break;
        }
        
        Vector3 pressedPosition = originalPosition + pressVector;
        Vector3 releasedPosition = originalPosition;
        
        Vector3 targetPosition = press ? pressedPosition : releasedPosition;
        
        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / pressDuration);
            
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            yield return null;
        }
        
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
        
        Collider col = GetComponent<Collider>();
        if (col is MeshCollider meshCol)
        {
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
        
        if (pokeVisualizer.TryGetComponent<Renderer>(out var renderer))
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0, 1, 0, 0.3f);
            renderer.material = mat;
        }
        
        Destroy(pokeVisualizer.GetComponent<Collider>());
    }
    
    public void TestToggle()
    {
        Debug.Log("<color=purple>Test toggle called</color>");
        ToggleScreen();
    }
}
