using UnityEngine;

public class ChestLidOpener : MonoBehaviour
{
    public GameObject lid; // Assign lid GameObject
    public RuneSnapZone[] snapZones;

    private bool lidOpened = false;

    int occupiedCount = 0;
    public void CheckAllSnapZones()
    {
        occupiedCount ++;
        if (occupiedCount == 4 && !lidOpened)
        {
            OpenLid();
            lidOpened = true;
        }
    }

    private void OpenLid()
    {
        // Simply remove the lid
        lid.SetActive(false);
    
    }
}
