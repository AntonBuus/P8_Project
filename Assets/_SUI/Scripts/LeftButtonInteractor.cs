using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeftButtonInteractor : ButtonInteractorBase
{
    [Header("Left Button Settings")]
    [Tooltip("Event triggered when the button is pressed")]
    public UnityEvent onButtonPress;

    [Tooltip("Event triggered when the button is released")]
    public UnityEvent onButtonRelease;

    [Header("Animation Settings")]
    [Tooltip("Animator controlling the button press animation")]
    public Animator buttonAnimator;

    [Tooltip("Name of the animator bool parameter")]
    public string pressParameterName = "IsPressed";

    private void Start()
    {
#if UNITY_XR
        Debug.Log("LeftButtonInteractor initialized (VR)");
#else
        Debug.Log("LeftButtonInteractor initialized");
#endif
    }

    protected override void InitializeButton()
    {
        // If needed, you can add specific logic for this button type
    }

    protected override bool CheckRaycastHit(RaycastHit hit)
    {
        bool valid = hit.transform == transform;
        Debug.Log($"CheckRaycastHit: {valid} (Hit: {hit.transform?.name}, Expected: {transform.name})");
        return valid;
    }

    protected override void OnLeftClickPressed(Vector2 mousePosition)
    {
        Debug.Log("OnLeftClickPressed called");

        onButtonPress?.Invoke();
        PlayInteractionSound();
        FlashMaterial();

        SetPressedTrue();

        Debug.Log($"Button {gameObject.name} pressed");
    }

    protected override void OnLeftClickDrag(Vector2 mousePosition)
    {
        // Optional: Log if drag is attempted
        Debug.Log("OnLeftClickDrag called - not implemented");
    }

    protected override void OnLeftClickReleased(Vector2 mousePosition)
    {
        Debug.Log("OnLeftClickReleased called");

        onButtonRelease?.Invoke();

        SetPressedFalse();

        Debug.Log($"Button {gameObject.name} released");
    }

    protected override void OnRightClickPressed(Vector2 mousePosition)
    {
        Debug.Log($"Button {gameObject.name} right-clicked");
    }

    public void SetPressedTrue()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool(pressParameterName, true);
            Debug.Log("Animator parameter set TRUE");
        }
        else
        {
            Debug.LogWarning("SetPressedTrue called, but buttonAnimator is null");
        }
    }

    public void SetPressedFalse()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool(pressParameterName, false);
            Debug.Log("Animator parameter set FALSE");
        }
        else
        {
            Debug.LogWarning("SetPressedFalse called, but buttonAnimator is null");
        }
    }
}
