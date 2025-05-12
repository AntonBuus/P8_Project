using UnityEngine;

public class IfAudioReady : MonoBehaviour
{
    public TTS_both_API _playRadioVoice;
    void Start()
    {
        if (_playRadioVoice == null)
        {
            _playRadioVoice = GameObject.Find("Radio TTS API").GetComponent<TTS_both_API>();
        }
    }
    bool audioHasPlayed = false;
    void Update()
    {
        if (!audioHasPlayed && _playRadioVoice.isAudioReady)
        {
            _playRadioVoice.InitializePlayAudio(_playRadioVoice.usableFilePath);
            audioHasPlayed = true;
            
        }
    }
}
