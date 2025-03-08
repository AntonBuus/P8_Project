using OpenAI;
using UnityEngine;

public class StartPromtps : MonoBehaviour
{
    private SupervisorInitialCall supervisorInitialCall;

    void Start()
    {
        supervisorInitialCall.SendReply();
    }
}
