using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeverTimeDial : MonoBehaviour
{
    [Header("=== Lever Settings ===")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public AudioSource pullSound;

    [Header("=== Scene To Load ===")]
    public string sceneToLoad = "BossOffice_1";

    private bool hasTriggered = false;

    private void Start()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnLeverReleased);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnLeverReleased);
        }
    }

    private void OnLeverReleased(SelectExitEventArgs args)
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (pullSound != null)
        {
            pullSound.Play();
        }

        Invoke(nameof(LoadScene), 1f); // Optional delay for sound
    }

    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
