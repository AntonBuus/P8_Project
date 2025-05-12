using UnityEngine;

public class PlayIfFailed : MonoBehaviour
{
    FindVoiceTent _playFailedVoice;
    TTS_both_API _supervisorVoice;
    CallOnceRadioHost _callOnceRadioHost;
    ChestLidOpener _chestLidOpener;
    bool audioHasPlayed = false;
    void Start()
    {
        if (_playFailedVoice == null)
        {
            _playFailedVoice = GameObject.Find("FindVoiceTent").GetComponent<FindVoiceTent>();
        }
        if (_supervisorVoice == null)
        {
            _supervisorVoice = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        }
        if (_callOnceRadioHost == null)
        {
            _callOnceRadioHost = GameObject.Find("CallOnceRadiohost").GetComponent<CallOnceRadioHost>();
        }
        if (_chestLidOpener == null)
        {
            _chestLidOpener = GameObject.Find("Rotation Lid").GetComponent<ChestLidOpener>();
        }
    }
    
    void Update()
    {
        if (!audioHasPlayed && _supervisorVoice.isThirdAudioReady)
        {
            _playFailedVoice.PlayRelevantVoice();
            _callOnceRadioHost.SendOneReply();
            audioHasPlayed = true;
            
        }
    }
}
