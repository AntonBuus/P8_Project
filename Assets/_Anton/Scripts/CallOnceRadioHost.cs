using OpenAI;
using UnityEngine;

public class CallOnceRadioHost : MonoBehaviour
{
    [SerializeField] CallRadiohost radiohost;

    bool _calledTextGen = false;

    void Start()
    {
        if (radiohost == null)
        {
            radiohost = GameObject.Find("Radiohost_API").GetComponent<CallRadiohost>();
        }

    }
    public void SendOneReply()
    {
        if (_calledTextGen == false)
        {
            radiohost.SendReply();
            _calledTextGen = true;
        }
    }

}
