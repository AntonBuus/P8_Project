using Unity.VisualScripting;
using UnityEngine;

public class PlayerEnteredArea : MonoBehaviour
{
    [SerializeField] private TTS_both_API _PlayGeneratedVoice;
    // [SerializeField] private string fetchedFilePath;
    private bool hasplayed = false;
    void Start()
    {
        if (_PlayGeneratedVoice == null)
        {
            _PlayGeneratedVoice = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        }
    }

    public UnityEngine.Events.UnityEvent onInvoke;

    private void OnTriggerEnter(Collider other)
    {
        if (_PlayGeneratedVoice.selectedTTSProvider == TTS_both_API.TTSProvider.No_Speech && other.CompareTag("Player") && !hasplayed)
        {
            Debug.Log("No Speech TTS Provider is selected.");
            NextPrompt();
        }
        else if (other.CompareTag("Player") && !hasplayed)
        {
            Debug.Log("Player entered the area!");
            // TTS_both_API _playGeneratedVoice = new TTS_both_API();
            _PlayGeneratedVoice.InitializePlayAudio(_PlayGeneratedVoice.usableFilePath);
            hasplayed = true;
            NextPrompt();
        }
    }
    private void NextPrompt()
    {
        if (onInvoke != null)
        {
            onInvoke.Invoke();
        }
    }
}
