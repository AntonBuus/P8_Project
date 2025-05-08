using UnityEngine;

public class RuneID : MonoBehaviour
{
    public string runeID = "I"; // Default ID, can be changed in the Inspector

    // This method can be called to get the Rune ID
    public string GetRuneID()
    {
        return runeID;
    }

    // This method can be called to set the Rune ID
    public void SetRuneID(string newID)
    {
        runeID = newID;
    }
}
