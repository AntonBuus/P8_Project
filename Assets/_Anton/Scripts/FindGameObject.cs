using OpenAI;
using UnityEngine;
using System.Collections;

public class FindGameObject : MonoBehaviour
{
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallSupervisor1 supervisor1;
    [SerializeField] private TTS_both_API ttsBothAPI;

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

        StartCoroutine(PlayClipsInSequence());
    }
    
    private IEnumerator PlayClipsInSequence()
    {
        audioSource.clip = firstClip;
        audioSource.Play();
        yield return new WaitForSeconds(firstClip.length);

        ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
    }

}
