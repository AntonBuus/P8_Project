using UnityEngine;
using TMPro;
using System.Collections;

public class ButtonDebug : MonoBehaviour
{
    public TextMeshPro countdownText;         // Text component for countdown
    public GameObject countdownDisplay;       // UI: countdown screen
    public GameObject missionDisplay;         // UI: mission screen

    private Coroutine countdownCoroutine;
    private static float remainingTime = -1f;
    private static bool timerStarted = false;

    private bool showingMission = false;
    private string currentScene = "";

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
        bool isMissionScene = currentScene == "SOPH_Hun_Era_1" || currentScene == "SOPH_Hun_Era_Tent_1";

        if (isMissionScene)
        {
            // ✅ Only start timer in SOPH_Hun_Era_1
            if (!timerStarted && currentScene == "SOPH_Hun_Era_1")
            {
                Debug.Log("✅ Timer auto-started in SOPH_Hun_Era_1");
                remainingTime = 300f;
                timerStarted = true;
                countdownCoroutine = StartCoroutine(StartCountdown());
            }

            // Show countdown UI by default in mission scenes
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
        bool isMissionScene = scene == "SOPH_Hun_Era_1" || scene == "SOPH_Hun_Era_Tent_1";

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
