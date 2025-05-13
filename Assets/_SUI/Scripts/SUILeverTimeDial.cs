using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SUILeverTimeDial : MonoBehaviour
{
    [Header("=== Scene Settings ===")]
    public string sceneToLoad = "BossOffice_1";
    public float sceneChangeDelay = 3f;

    [Header("=== Effects ===")]
    public ParticleSystem bootupEffectPrefab;
    public AudioSource pullSound;

    [Header("=== Lever Reference ===")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    [Header("=== Particle Spawn Location ===")]
    public Transform pipboiTransform;

    [Header("=== Lever Return ===")]
    public Transform attachPoint; // 👈 Your custom reset position

    private bool hasActivated = false;
    private bool isReturning = false;

    private void Start()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnLeverPulled);
            grabInteractable.selectExited.AddListener(OnLeverReleased);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnLeverPulled);
            grabInteractable.selectExited.RemoveListener(OnLeverReleased);
        }
    }

    private void OnLeverPulled(SelectEnterEventArgs args)
    {
        if (hasActivated) return;
        hasActivated = true;

        if (pullSound != null)
            pullSound.Play();

        if (bootupEffectPrefab != null && pipboiTransform != null)
        {
            ParticleSystem ps = Instantiate(bootupEffectPrefab, pipboiTransform.position, Quaternion.identity);
            ps.Play();
        }
        
        // Scene switching moved to OnLeverReleased
    }

    private void OnLeverReleased(SelectExitEventArgs args)
    {
        isReturning = true;
        
        // Only trigger scene switching if the lever has been activated
        if (hasActivated)
        {
            StartCoroutine(SwitchSceneAfterDelay());
        }
    }

    private void Update()
    {
        if (isReturning && attachPoint != null)
        {
            transform.position = Vector3.Lerp(transform.position, attachPoint.position, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, attachPoint.rotation, Time.deltaTime * 5f);

            if (Vector3.Distance(transform.position, attachPoint.position) < 0.001f &&
                Quaternion.Angle(transform.rotation, attachPoint.rotation) < 0.5f)
            {
                isReturning = false;
            }
        }
    }

    private IEnumerator SwitchSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneChangeDelay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
