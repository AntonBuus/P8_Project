using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
// Purpose: Lets a specific card “snap” into the correct slot.

public class RuneSnapZone : MonoBehaviour
{
    public string acceptedTag = "RuneCard"; // Only accept objects with this tag
    public bool isOccupied = false; // Has the slot already been filled?
    public ChestLidOpener lidOpener; // Reference to the chest controller

    public string RuneID = "I"; // Expected rune ID for this slot

    private void OnTriggerEnter(Collider other)
    {
        // Get the RuneID script from the object
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

             // Stop physics on the object
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics
            }
        }
    }
}
