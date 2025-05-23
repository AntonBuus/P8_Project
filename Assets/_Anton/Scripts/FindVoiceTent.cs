using OpenAI;
using UnityEngine;
using System.Collections;

public class FindVoiceTent : MonoBehaviour
{
    
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallSupervisor1 supervisor1;
    [SerializeField] private TTS_both_API ttsBothAPI;
    [SerializeField] private AdjustablePromptsRadioHost _APRH; 
    bool _playedLastPrompt = false;

    public AudioSource audioSource;
    public AudioClip firstClip;
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

            StartCoroutine(PlayClipsInSequence());
            _playedLastPrompt = true;
            _APRH.CollectRadioContent();
        }
    }
    private IEnumerator PlayClipsInSequence()
    {
        audioSource.clip = firstClip;
        audioSource.Play();
        yield return new WaitForSeconds(firstClip.length);

        ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
    }
}
