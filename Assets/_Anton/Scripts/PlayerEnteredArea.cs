using UnityEngine;

public class PlayerEnteredArea : MonoBehaviour
{
    private Supervisor_TTS_playht CallPlayHT;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the area!");
            
        }
    }
}
