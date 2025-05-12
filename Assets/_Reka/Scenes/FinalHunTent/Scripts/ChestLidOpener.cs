using UnityEngine;
// Purpose: Opens the chest lid when all rune cards are correctly placed.

public class ChestLidOpener : MonoBehaviour
{
    public GameObject lid; // Assign lid GameObject
    // public RuneSnapZone[] snapZones; // List of all snap slots on the chest
    [SerializeField] MissionTimeChecker _missionTimeChecker; // Reference to the mission time checker
    [SerializeField] CompletedObjective _completedObjective; // Reference to the completed objective
    private bool lidOpened = false; // Track if the lid has already opened

    int occupiedCount = 0; // How many slots are filled

    private float _rotationSpeed = 200f;
    private float _targetAngle = 80f;
    private float _closedAngle = 0f;
    public bool _openlid = false; // Used to trigger the animation once
    public GameObject _objectToEnable; // Object to enable when the lid opens

    void Update()
    {
        float targetAngle = _openlid ? _targetAngle : _closedAngle;
        Quaternion targetRotation = Quaternion.Euler(targetAngle, transform.localEulerAngles.y, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
    public void CheckAllSnapZones()
    {
        // Count a newly occupied slot
        occupiedCount ++;

        // If all 4 slots are correct and the lid isn't opened yet
        if (occupiedCount == 4 && !lidOpened)
        {
            OpenLid(); // Trigger the lid to open
            lidOpened = true; // Prevent re-opening
        }
    }

    private void OpenLid()
    {
        // Simply remove the lid
        // lid.SetActive(false);
        _openlid = true; // Trigger the lid opening animation    
        _objectToEnable.SetActive(true); // Hide the object to retrieve
        _completedObjective.InvokeOnceInThisScene(); // Invoke the completed objective
        _missionTimeChecker.CheckMissionTime(); // Check the mission time
        
    }
}

// using UnityEngine;

// // Purpose: Opens the chest lid with smooth animation when all rune cards are placed
// public class ChestLidOpener : MonoBehaviour
// {
//     public GameObject lid; // The lid object to animate
//     public RuneSnapZone[] snapZones; // All rune slot zones

//     private bool lidOpened = false;
//     private bool openLid = false; // Used to trigger the animation once
//     private int occupiedCount = 0;

//     public float rotationSpeed = 200f; // Speed of lid opening
//     public float targetAngle = -90f; // Angle to open the lid to
//     public float closedAngle = 0f; // Starting closed angle

//     void Update()
//     {
//         if (openLid && lid != null)
//         {
//             // You can switch to Z-axis if this doesn't work visually
//             Quaternion targetRotation = Quaternion.Euler(targetAngle, 0f, 0f);

//             lid.transform.localRotation = Quaternion.RotateTowards(
//                 lid.transform.localRotation,
//                 targetRotation,
//                 rotationSpeed * Time.deltaTime
//             );

//             if (Quaternion.Angle(lid.transform.localRotation, targetRotation) < 0.5f)
//             {
//                 lid.transform.localRotation = targetRotation;
//                 openLid = false; // Stop rotating once lid is fully opened
//             }
//         }
//     }

//     public void CheckAllSnapZones()
//     {
//         occupiedCount++;

//         if (occupiedCount == snapZones.Length && !lidOpened)
//         {
//             lidOpened = true;
//             openLid = true;
//         }
//     }
// }
