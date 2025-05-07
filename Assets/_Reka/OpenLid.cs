using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OpenLid : MonoBehaviour
{
    [Header("Lid Settings")]
    public bool IsOpen = false;
    public float OpenAngle = -90f;
    public float Speed = 2f;

    [Header("Optional: Trigger by Grabbing")]
    public XRGrabInteractable grabObject;
    public float DelayBeforeOpen = 1f;

    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private bool isTriggered = false;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (IsOpen && !isTriggered)
        {
            targetRotation = Quaternion.Euler(OpenAngle, 0, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * Speed);
        }
    }

    public void TriggerOpenWithDelay()
    {
        if (!isTriggered)
            StartCoroutine(OpenAfterDelay());
    }

    private System.Collections.IEnumerator OpenAfterDelay()
    {
        isTriggered = true;
        yield return new WaitForSeconds(DelayBeforeOpen);
        IsOpen = true;
    }

    public void OpenLidNow() // Call this from puzzle logic
    {
        IsOpen = true;
        isTriggered = false;
    }
}
