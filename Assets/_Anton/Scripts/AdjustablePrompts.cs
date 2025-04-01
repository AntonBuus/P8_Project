using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class AdjustablePrompts : MonoBehaviour
{

    [Header("Main variables for prompts")]
    public string _era = "The Hun Era"; // Default era
    public string _anomalyObject = "A Walkman from the 80s"; // Default object
    public string _wasObjectRetrieved = "The agent brings back the object"; // Default retrieval status
    public string _spottedStatus = "He was not spotted, so no disruptions were caused to the timeline, very good performance"; // Default spotted status

    [Header("Prompt variables")]
    [TextArea(3, 10)] public string _prompt2ArrivalInEra;
    [TextArea(3, 10)] public string _prompt3MissionReport;

    void Start()
    {
        CollectArrivalPrompt(); 
        CollectMissionReportPrompt();
    }
    public void CollectArrivalPrompt()
    {
        _prompt2ArrivalInEra = "The agent travels to " + _era +". You have received new information about the object: It is" + _anomalyObject + ". "
        + "Inform the agent and make a comment about how this particular object could influence the time period."
        + "Tell him to blend in while searching for the item.";
    }
    public void CollectMissionReportPrompt()
    {
        _prompt3MissionReport = _wasObjectRetrieved + ". " + _spottedStatus
        + " Address the situation and call him back to the office.";
    }

}



