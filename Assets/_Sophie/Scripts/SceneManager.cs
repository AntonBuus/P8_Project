using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    [Header("=== Scene Trigger Settings ===")]
    public bool useBoxTrigger = false;
    public string sceneToLoad;
    public string spawnPointTag;

    [Header("=== Lever-Based Time Travel Settings ===")]
    public bool useLever = false;

    [Header("=== Time Travel Effect (For Lever Only) ===")]
    public float delayBeforeSceneLoad = 2f;
    public AudioSource timeTravelAudio;
    public ParticleSystem timeTravelEffect;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!useBoxTrigger || hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        Debug.Log("[SceneManager] OnTriggerEnter detected. Preparing to save scene and load: " + sceneToLoad);
        SaveSceneAndSpawn(spawnPointTag);

        UnitySceneManager.LoadScene(sceneToLoad);
    }

    public void ForceSceneShiftFromLever()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        StartCoroutine(TriggerTimeTravelSequence());
    }

    private IEnumerator TriggerTimeTravelSequence()
    {
        SaveSceneAndSpawn(spawnPointTag);

        if (timeTravelAudio != null)
            timeTravelAudio.Play();

        if (timeTravelEffect != null)
            timeTravelEffect.Play();

        yield return new WaitForSeconds(delayBeforeSceneLoad);

        UnitySceneManager.LoadScene(sceneToLoad);
    }

    private void SaveSceneAndSpawn(string spawn)
    {
        string currentScene = UnitySceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentScene);
        Debug.Log("[SceneManager] Saved LastScene as: " + currentScene);

        if (!string.IsNullOrEmpty(spawn))
        {
            PlayerPrefs.SetString("SpawnPoint", spawn);
            Debug.Log("[SceneManager] Saved SpawnPoint as: " + spawn);
        }

        PlayerPrefs.Save();
    }
}
