using UnityEngine;

public class MissionTimeChecker : MonoBehaviour
{
    [SerializeField] AdjustablePrompts _adjustablePrompts; 
    [SerializeField] LeverTimeDial _leverTimeDial; 
    float _currentTime; 
    public float _middleTime = 120; 
    [SerializeField] ButtonDebug _missionTime; 
    public string _winSceneName = "Win_Scene_v3" ; 
    public string _failSceneName = "Fail_Scene_v3"; 

    public bool _failedOficially = false; 

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


    public void CheckMissionTime()
    {
        _currentTime = _missionTime.SyncedRemainingTime; 
        
        if (_currentTime <= 0)
        {
            _adjustablePrompts.SetTimeStatus(2); 
            _adjustablePrompts.ObjectWasNotRetrieved(); 
            _leverTimeDial.sceneToLoad = _failSceneName; 
            Debug.Log("called failed missiontime"); 
            _failedOficially = true; 
        }
        if (_currentTime <= _middleTime && _currentTime >0) 
        {
            _adjustablePrompts.SetTimeStatus(1); 
            _leverTimeDial.sceneToLoad = _winSceneName; 
            Debug.Log("called middle missiontime");
        }
        if (_currentTime > _middleTime) 
        {
            _adjustablePrompts.SetTimeStatus(0); 
            _leverTimeDial.sceneToLoad = _winSceneName; 
            Debug.Log("called normal missiontime");
        }
        _adjustablePrompts.CollectMissionReportPrompt(); 
    }
}
