using OpenAI;
using UnityEngine;

public class FindVoiceEndscene : MonoBehaviour
{
    //This is to be inserted to any script that needs to influence or activate prompts
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallRadiohost radiohost;
    [SerializeField] private TTS_both_API ttsBothAPI;
    bool _playedLastPrompt = false;
    void Start()
    {
        if (adjustablePrompts == null)
        {
            adjustablePrompts = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
        if (radiohost == null)
        {
            radiohost = GameObject.Find("Radiohost_API").GetComponent<CallRadiohost>();
        }
        if (ttsBothAPI == null)
        {
            ttsBothAPI = GameObject.Find("Radio TTS API").GetComponent<TTS_both_API>();
        }


    }
    public void PlayRelevantVoice()
    {
        if (_playedLastPrompt == false)
        {
            ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
            _playedLastPrompt = true;
        }
    }

}
