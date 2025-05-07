using UnityEngine;

public class IfAudioReady : MonoBehaviour
{
    public TTS_both_API _playGeneratedVoice;
    bool audioHasPlayed = false;
    void Update()
    {
        if (!audioHasPlayed && _playGeneratedVoice.isAudioReady)
        {
            _playGeneratedVoice.InitializePlayAudio(_playGeneratedVoice.usableFilePath);
            audioHasPlayed = true;
            
        }
    }
}
