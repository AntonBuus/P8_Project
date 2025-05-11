using UnityEngine;
using TMPro;
using System.Collections;

public class ButtonDebug : MonoBehaviour
{
    public TextMeshPro countdownText;         // Text component for countdown
    public GameObject countdownDisplay;       // UI: countdown screen (e.g. TMP text)
    public GameObject missionDisplay;         // UI: mission screen (now an Image or any GameObject)

    private Coroutine countdownCoroutine;
    private static float remainingTime = -1f;
    private static bool timerStarted = false;

    private bool showingMission = false;
    private string currentScene = "";

    MissionTimeChecker _missionTimeChecker;
    public float SyncedRemainingTime
    {
        get { return remainingTime; }
        set { remainingTime = value; }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(UpdateCountdownTextContinuously());

        if (timerStarted && countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(StartCountdown());
        }
    }

    void Start()
    {
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isMissionScene = currentScene == "Hun_Era_v3" || currentScene == "Hun_Era_Tent_v3";

        if (isMissionScene)
        {
            // ✅ Only start timer in Hun_Era_v3
            if (!timerStarted && currentScene == "Hun_Era_v3")
            {
                Debug.Log("✅ Timer auto-started in Hun_Era_v3");
                remainingTime = 30f;
                timerStarted = true;
                countdownCoroutine = StartCoroutine(StartCountdown());
            }

            // Show countdown UI by default
            if (countdownDisplay != null) countdownDisplay.SetActive(true);
            if (missionDisplay != null) missionDisplay.SetActive(false);
        }
        else
        {
            // ❌ In all other scenes (like SOPH_IntroScene_1), hide both UIs
            if (countdownDisplay != null) countdownDisplay.SetActive(false);
            if (missionDisplay != null) missionDisplay.SetActive(false);
        }
    }

    public void ToggleDisplay()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isMissionScene = scene == "Hun_Era_v3" || scene == "Hun_Era_Tent_v3";

        if (!isMissionScene)
        {
            Debug.Log("🟡 Toggle ignored: not in mission scene.");
            return;
        }

        showingMission = !showingMission;

        if (countdownDisplay != null)
            countdownDisplay.SetActive(!showingMission);

        if (missionDisplay != null)
            missionDisplay.SetActive(showingMission);

        Debug.Log(showingMission ? "📋 Showing mission UI" : "⏱️ Showing countdown again");
    }

    private IEnumerator StartCountdown()
    {
        float endTime = Time.realtimeSinceStartup + remainingTime;

        while (remainingTime > 0f)
        {
            remainingTime = endTime - Time.realtimeSinceStartup;
            remainingTime = Mathf.Max(0f, remainingTime);

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            if (countdownText != null && countdownDisplay.activeSelf)
                countdownText.text = $"{minutes:D2}:{seconds:D2}";

            yield return new WaitForSeconds(1f);
        }
        
        if (currentScene == "Hun_Era_Tent_v3")
        {
           _missionTimeChecker = GameObject.Find("MissionTimeChecker").GetComponent<MissionTimeChecker>();
            _missionTimeChecker.CheckMissionTime(); // Call the mission time checker
        }
        if (countdownText != null)
            countdownText.text = "Time's up!";
    }

    private IEnumerator UpdateCountdownTextContinuously()
    {
        while (true)
        {
            if (countdownText == null)
            {
                GameObject tmpObj = GameObject.Find("CountdownDisplay");
                if (tmpObj != null)
                {
                    countdownText = tmpObj.GetComponent<TextMeshPro>();
                    Debug.Log("✅ Reconnected CountdownDisplay");
                }
                else
                {
                    countdownText = FindObjectOfType<TextMeshPro>();
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
