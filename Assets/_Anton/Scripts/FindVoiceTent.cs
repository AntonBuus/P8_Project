using OpenAI;
using UnityEngine;

public class FindVoiceTent : MonoBehaviour
{
    //This is to be inserted to any script that needs to influence or activate prompts
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallSupervisor1 supervisor1;
    [SerializeField] private TTS_both_API ttsBothAPI;
    [SerializeField] private AdjustablePromptsRadioHost _APRH; // Reference to the output text field
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
        if (_APRH == null)
        {
            _APRH = GameObject.Find("Radiohost PromptCollection").GetComponent<AdjustablePromptsRadioHost>();
        }

    }
    public void PlayRelevantVoice()
    {
        if (_playedLastPrompt == false)
        {
            ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
            _playedLastPrompt = true;
            _APRH.CollectRadioContent();
        }
    }
}
