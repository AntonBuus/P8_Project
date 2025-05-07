using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using UnityEngine.UI;
using OpenAI;
using UnityEditor.EditorTools;

public class AdjustablePromptsRadioHost : MonoBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private CallRadiohost _callRadiohost; // Reference to the output text field

    [SerializeField] private AdjustablePrompts _adjustablePromptsSupervisor; // Reference to the TTS system


    // [Header("Main variables for prompts")]

    // public string _wasObjectRetrieved = "The agent brings back the object"; // Default retrieval status

    // public string _timeStatus = "They brought back the object in due time"; // Default time status

    [Header("Prompt variables")]
    [TextArea(3, 10)] public string _prompt1RadiohostReport;

    void Start()
    {
        if (_adjustablePromptsSupervisor == null)
        {
            _adjustablePromptsSupervisor = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
        CollectRadioContent();
    }
    public void CollectRadioContent()
    {
        _prompt1RadiohostReport = "The agent returned from his trip to " + _adjustablePromptsSupervisor._era + " with the object: "
        + _adjustablePromptsSupervisor._anomalyObject + ". " + _adjustablePromptsSupervisor._timeStatus;
        _inputField.text = _prompt1RadiohostReport; // Set the input field text to the prompt
        Debug.Log("Called collect arrival"); // Log the prompt to the console for debugging
        // _callRadiohost.SendReply();
    }
    
}



