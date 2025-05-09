using UnityEngine;
using TMPro;
using System.Collections;

public class ButtonDebug : MonoBehaviour
{
    public TextMeshPro countdownText;
    private Coroutine countdownCoroutine;

    private static float remainingTime = -1f;
    private static bool timerStarted = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(UpdateCountdownTextContinuously());

        if (timerStarted && countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(StartCountdown());
        }
    }

    public void OnHover()
    {
        TriggerTimer();
    }

    public void TriggerTimer()
    {
        if (!timerStarted)
        {
            remainingTime = 300f;
            timerStarted = true;
            countdownCoroutine = StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        while (remainingTime > 0f)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            if (countdownText != null)
                countdownText.text = $"{minutes:D2}:{seconds:D2}";

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
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
