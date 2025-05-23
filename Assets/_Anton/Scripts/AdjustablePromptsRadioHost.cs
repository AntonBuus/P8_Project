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
    [SerializeField] private CallRadiohost _callRadiohost; 

    [SerializeField] private AdjustablePrompts _adjustablePromptsSupervisor; 

    [Header("Prompt variables")]
    [TextArea(3, 10)] public string _prompt1RadiohostReport;

    void Start()
    {
        if (_adjustablePromptsSupervisor == null)
        {
            _adjustablePromptsSupervisor = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
        
    }
    public void CollectRadioContent()
    {
        _prompt1RadiohostReport = "The agent returned from his trip to " + _adjustablePromptsSupervisor._era + ". The object was: "
        + _adjustablePromptsSupervisor._anomalyObject + ". "+ _adjustablePromptsSupervisor._wasObjectRetrieved + _adjustablePromptsSupervisor._timeStatus;
        _inputField.text = _prompt1RadiohostReport; 
        Debug.Log("Collected radiohost prompt");
    }
    
}



