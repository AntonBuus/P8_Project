// using UnityEngine;

// public class OpenLid : MonoBehaviour
// {
//     public bool isOpen = false;
//     public float openAngle = -90f;  // Adjust this if your lid opens differently
//     public float speed = 2f;
//     private Quaternion closedRotation;
//     private Quaternion openRotation;

//     void Start()
//     {
//         closedRotation = transform.localRotation;
//         openRotation = Quaternion.Euler(openAngle, 0, 0);  // May need tweaking based on pivot
//     }

//     void Update()
//     {
//         Quaternion targetRotation = isOpen ? openRotation : closedRotation;
//         transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
//     }

//     public void ToggleLid()
//     {
//         isOpen = !isOpen;
//     }
// }

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenLid : MonoBehaviour
{
    [Header("Lid Settings")]
    public bool isOpen = false;
    public float openAngle = -90f;
    public float speed = 2f;

    [Header("Optional: Trigger by Grabbing")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabObject; // Drag the paper here
    public float delayBeforeOpen = 1f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(openAngle, 0f, 0f) * closedRotation;

        if (grabObject != null)
        {
            grabObject.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnDestroy()
    {
        if (grabObject != null)
        {
            grabObject.selectEntered.RemoveListener(OnGrabbed);
        }
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Invoke(nameof(UnlockLid), delayBeforeOpen);
    }

    public void UnlockLid()
    {
        isOpen = true;
    }
}
