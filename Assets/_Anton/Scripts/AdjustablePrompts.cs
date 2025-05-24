using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using OpenAI;
using UnityEditor.EditorTools;

public class AdjustablePrompts : MonoBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private CallSupervisor1 SupervisorAPICall; 

    [Header("Main variables for prompts")]
    public string _era = "The Hun Era"; 
    public string _anomalyObject = "A moderen crossbow"; 
    public bool _setObjectRandomly = false; 
    public string _wasObjectRetrieved = "The agent reached the object"; 
    [Tooltip("Flag to check if the object is retrieved")] public bool _objectIsRetrieved = false;
    public string _timeStatus = "They made it in due time, so no disruptions were caused to the timeline, very good performance"; 

    [Header("Prompt variables")]
    [TextArea(3, 10)] public string _prompt2ArrivalInEra;
    [TextArea(3, 10)] public string _prompt3MissionReport;

    void Start()
    {
        if(_setObjectRandomly)
        {
            SetAnomalyObject(); 
        }
    }
    public void CollectArrivalPrompt()
    {
        _prompt2ArrivalInEra = "The agent travels to " + _era +". You have received new information about the object: It is" + _anomalyObject + ". "
        + "Inform the agent and make a comment about how this particular object could influence the time period."
        + "Tell him to find some high ground and get an overview of the area while searching for the item.";
        _inputField.text = _prompt2ArrivalInEra; 
        Debug.Log("Called collect arrival"); 
        SupervisorAPICall.SendReply();
    }
    public void CollectMissionReportPrompt()
    {
        _prompt3MissionReport = _wasObjectRetrieved + ". " + _timeStatus
        + " Address the situation and call him back to the office.";
        _inputField.text = _prompt3MissionReport; 
        Debug.Log("Called collect mission report"); 
        SupervisorAPICall.SendReply();
    }
    public void SetAnomalyObject()
    {
        string[] anomalyObjects = { "A Walkman from the 80s", "A smartphone", 
        "A medieval sword", "A modern olimpic-grade crossbow", "A computer from the 90s" };
        _anomalyObject = anomalyObjects[UnityEngine.Random.Range(0, anomalyObjects.Length)];
        Debug.Log("Anomaly object set to: " + _anomalyObject);
    }

    public void ObjectWasNotRetrieved() 
    {
        _wasObjectRetrieved = "The agent failed to retrieve the object in time. ";
    }

    public void SetTimeStatus(int _timeStatusLevel)
    {
        
        if (_timeStatusLevel == 0)
        {
            _timeStatus = "They made it in due time, so no disruptions were caused to the timeline, very good performance"; 
        }
        else if (_timeStatusLevel == 1)
        {
            _timeStatus = "They were late reaching the object, this could cause some mild disruptions to the timeline, otherwise a good performance.";
        }
        else if (_timeStatusLevel == 2)
        {
            _timeStatus = "This will cause catastrophic changes to the timeline"; 
        }
        else
        {
            _timeStatus = "You are not sure whether they were late or not, so you can check up on it later";
        }
    }
    
   
}



