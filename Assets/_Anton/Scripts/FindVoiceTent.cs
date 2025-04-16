using OpenAI;
using UnityEngine;

public class FindVoiceTent : MonoBehaviour
{
    //This is to be inserted to any script that needs to influence or activate prompts
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallSupervisor1 supervisor1;
    [SerializeField] private TTS_both_API ttsBothAPI;
    bool _playedLastPrompt = false;
    void Start()
    {
        if (adjustablePrompts == null)
        {
            adjustablePrompts = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
        if (supervisor1 == null)
        {
            supervisor1 = GameObject.Find("Supervisor_Initial_API").GetComponent<CallSupervisor1>();
        }
        if (ttsBothAPI == null)
        {
            ttsBothAPI = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        }


    }
    public void playRelevantVoice()
    {
        if (_playedLastPrompt == false)
        {
            ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
            _playedLastPrompt = true;
        }
    }

}
