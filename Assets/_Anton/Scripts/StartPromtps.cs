using OpenAI;
using UnityEngine;

public class StartPromtps : MonoBehaviour
{
    [SerializeField] private CallSupervisor1 CallOpenAICompletion;

    void Start()
    {
        // CallOpenAICompletion = GetComponent<CallSupervisor1>();
        CallOpenAICompletion.SendReply();
    }
}
