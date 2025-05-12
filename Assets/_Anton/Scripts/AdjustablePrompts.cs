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
    [SerializeField] private CallSupervisor1 SupervisorAPICall; // Reference to the output text field

    [Header("Main variables for prompts")]
    public string _era = "The Hun Era"; // Default era
    public string _anomalyObject = "A moderen crossbow"; // Default object
    public bool _setObjectRandomly = false; // Flag to set object randomly
    public string _wasObjectRetrieved = "The agent reached the object"; // Default retrieval status
    [Tooltip("Flag to check if the object is retrieved")] public bool _objectIsRetrieved = false;
    public string _timeStatus = "They made it in due time, so no disruptions were caused to the timeline, very good performance"; // Default time status

    [Header("Prompt variables")]
    [TextArea(3, 10)] public string _prompt2ArrivalInEra;
    [TextArea(3, 10)] public string _prompt3MissionReport;

    void Start()
    {
        // CollectArrivalPrompt(); 
        // CollectMissionReportPrompt();
        if(_setObjectRandomly)
        {
            SetAnomalyObject(); // Set a random anomaly object
        }
    }
    public void CollectArrivalPrompt()
    {
        _prompt2ArrivalInEra = "The agent travels to " + _era +". You have received new information about the object: It is" + _anomalyObject + ". "
        + "Inform the agent and make a comment about how this particular object could influence the time period."
        + "Tell him to find some high ground and get an overview of the area while searching for the item.";
        _inputField.text = _prompt2ArrivalInEra; // Set the input field text to the prompt
        Debug.Log("Called collect arrival"); // Log the prompt to the console for debugging
        SupervisorAPICall.SendReply();
    }
    public void CollectMissionReportPrompt()
    {
        _prompt3MissionReport = _wasObjectRetrieved + ". " + _timeStatus
        + " Address the situation and call him back to the office.";
        _inputField.text = _prompt3MissionReport; // Set the input field text to the prompt
        Debug.Log("Called collect mission report"); // Log the prompt to the console for debugging'
        SupervisorAPICall.SendReply();
    }
    public void SetAnomalyObject()
    {
        string[] anomalyObjects = { "A Walkman from the 80s", "A smartphone", 
        "A medieval sword", "A modern olimpic-grade crossbow", "A computer from the 90s" };
        _anomalyObject = anomalyObjects[UnityEngine.Random.Range(0, anomalyObjects.Length)];
        Debug.Log("Anomaly object set to: " + _anomalyObject);
    }

    public void ObjectWasNotRetrieved() //subbed for SetObjectRetrieved()
    {
        _wasObjectRetrieved = "The agent failed to retrieve the object in time. ";
    }

    public void SetTimeStatus(int _timeStatusLevel)
    {
        // Set the time status based on the level of lateness
        if (_timeStatusLevel == 0)
        {
            _timeStatus = "They made it in due time, so no disruptions were caused to the timeline, very good performance"; // Default time status
        }
        else if (_timeStatusLevel == 1)
        {
            _timeStatus = "They were late reaching the object, this could cause some mild disruptions to the timeline, otherwise a good performance.";
        }
        else if (_timeStatusLevel == 2)
        {
            _timeStatus = "This will cause catastrophic changes to the timeline"; // blank because if they are too late they did not bring back the object so there is no point in saying anything about it
        }
        else
        {
            _timeStatus = "You are not sure whether they were late or not, so you can check up on it later";
        }
    }
    
    // Preserving the object retrieval status for future use

    // public void SetObjectRetrieved() 
    // {
    //     if (_objectIsRetrieved)
    //     {
    //         _wasObjectRetrieved = "The agent reached the object";
    //     }
    //     else
    //     {
    //         _wasObjectRetrieved = "The agent failed to retrieve the object";
    //     }
    //     Debug.Log("Object retrieved status set to: " + _objectIsRetrieved);
    // }

    // We are holding off on spotted status for now, but it is here for future use
    // public string _spottedStatus = "He was not spotted, so no disruptions were caused to the timeline, very good performance"; // Default spotted status

    // public void SetSpottedStatus(int _spottedLevel)
    // {

    //     if (_spottedLevel == 0)
    //     {
    //         _spottedStatus = "He was not spotted, so no disruptions were caused to the timeline, very good performance";
    //     }
    //     else if (_spottedLevel == 1)
    //     {
    //         _spottedStatus = "He was spotted so it might cause some disruptions, but overall a good performance.";
    //     }
    //     else if (_spottedLevel == 2)
    //     {
    //         _spottedStatus = "He was spotted too many times, this could cause some rather large disruptions.";
    //     }
    //     else
    //     {
    //         _spottedStatus = "You are not sure whether he was spotted or not, so you can check up on it later";
    //     }
    //     Debug.Log("Spotted level set to: " + _spottedLevel);
    // }
}



