using TMPro;
using UnityEngine;

public class SetPromptParameters : MonoBehaviour
{
    public TMP_Text _parameter1;
    public TMP_Text _parameter2;

    public TMP_Text _initialPrompt;


    private void Start()
    {
        ReloadInitialprompt();
    }
    public void ReloadInitialprompt()
    {
        _initialPrompt.text = "You are the supervisor in a timetraveling company and you are directing an agent retrieving timeanomaly-objects from timeperiods. Send him to " + _parameter2.text + " where today's object is located. Let him know that you are getting more intel on what the object will be. Your mood is " + _parameter1.text + " so form dialouge accordingly";
        //_initialPrompt.text = "Create diaglouge based on the following fact: You are a supervisor in a timetraveling company and you are directing an agent retrieving timeanomaly-objects from timeperiods. Greet him, and send him to " + _parameter2.text + " where today's object is located. Let him know that you are getting more intel on what the object will be. Keep it short";

    }
}
