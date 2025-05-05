using UnityEngine;
using TMPro;
using System.Collections;

public class ButtonDebug : MonoBehaviour
{
    public TextMeshPro countdownText; // Assign this in the Inspector
    private Coroutine countdownCoroutine;

    public void LogPress()
    {
        Debug.Log("Push button was pressed!");

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(StartCountdown(300)); // 5 minutes = 300 seconds
    }

    private IEnumerator StartCountdown(int totalSeconds)
    {
        int timeLeft = totalSeconds;

        while (timeLeft >= 0)
        {
            int minutes = timeLeft / 60;
            int seconds = timeLeft % 60;

            countdownText.text = $"{minutes:D2}:{seconds:D2}";

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        countdownText.text = "Time's up!";
    }
}
