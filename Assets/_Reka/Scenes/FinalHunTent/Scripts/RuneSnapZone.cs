using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RuneSnapZone : MonoBehaviour
{
    public string acceptedTag = "RuneCard";
    public bool isOccupied = false;
    public ChestLidOpener lidOpener;

    public string RuneID = "I";

    private void OnTriggerEnter(Collider other)
    {
        RuneID runeIDComponent = other.GetComponent<RuneID>();
        if (!isOccupied && other.CompareTag(acceptedTag)&& runeIDComponent != null && runeIDComponent.runeID == RuneID)
        {
            // Snap object to this position
            other.transform.position = transform.position;
            other.transform.rotation = transform.rotation;

            // Optional: disable grabbing
            var grab = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null) grab.enabled = false;

            isOccupied = true;
            lidOpener.CheckAllSnapZones();
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics
            }
        }
    }
}
