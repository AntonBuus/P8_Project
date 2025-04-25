using OpenAI;
using UnityEngine;

public class FindGameObject : MonoBehaviour
{
    //This is to be inserted to any script that needs to influence or activate prompts
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    [SerializeField] private CallSupervisor1 supervisor1;
    [SerializeField] private TTS_both_API ttsBothAPI;
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
        ttsBothAPI.InitializePlayAudio(ttsBothAPI.usableFilePath);
        adjustablePrompts.CollectMissionReportPrompt();

    }


}
