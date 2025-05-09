using UnityEngine;

public class MissionTimeChecker : MonoBehaviour
{
    [SerializeField] AdjustablePrompts _adjustablePrompts; // Reference to the TTS system
    [SerializeField] LeverTimeDial _leverTimeDial; // Reference to the lever time dial
    float _currentTime; // Current time in seconds
    public float _middleTime = 180; // Time in seconds to warn the player
    [SerializeField] ButtonDebug _missionTime; // Reference to the ButtonDebug script
    public string _winSceneName = "Win_Scene_v3" ; // Name of the win scene
    public string _failSceneName = "Fail_Scene_v3"; // Name of the lose scene

    void Start()
    {
        if (_adjustablePrompts == null)
        {
            _adjustablePrompts = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
        if (_leverTimeDial == null)
        {
            _leverTimeDial = GameObject.Find("Pull").GetComponent<LeverTimeDial>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // CheckMissionTime(); // Call the method to check the mission time
    }

    public void CheckMissionTime()
    {
        _currentTime = _missionTime.SyncedRemainingTime; // Update _currentTime with the value from the external source
        Debug.Log("called check missiontime"); // Log the current time for debugging
        if (_currentTime <= 0)
        {
            _adjustablePrompts.SetTimeStatus(2); 
            _adjustablePrompts.ObjectWasNotRetrieved(); 
            _leverTimeDial.sceneToLoad = _failSceneName; // Set the scene to load to the lose scene
        }
        if (_currentTime <= _middleTime) // Check if the time is below x amount
        {
            _adjustablePrompts.SetTimeStatus(1); 
            _leverTimeDial.sceneToLoad = _winSceneName; // Set the scene to load to the win scene
        }
        if (_currentTime > _middleTime) // Check if the time is above x amount
        {
            _adjustablePrompts.SetTimeStatus(0); 
            _leverTimeDial.sceneToLoad = _winSceneName; // Set the scene to load to the win scene
        }
    }
}
