using UnityEngine;

public class PlayerEnteredArea : MonoBehaviour
{
    [SerializeField] private TTS_both_API PlayGeneratedVoice;
    // [SerializeField] private string fetchedFilePath;
    private bool hasplayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasplayed)
        {
            Debug.Log("Player entered the area!");
            // TTS_both_API _playGeneratedVoice = new TTS_both_API();
            PlayGeneratedVoice.InitializePlayAudio(PlayGeneratedVoice.usableFilePath);
            hasplayed = true;
        }
    }
}
