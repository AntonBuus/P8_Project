using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    [Header("=== Scene Trigger Settings ===")]
    public bool useBoxTrigger = true;
    public string sceneToLoad;
    public string spawnPointTag;

    [Header("=== Lever-Based Time Travel Settings ===")]
    public bool useLever = false;
    public float leverValue;
    public float leverTolerance = 5f;

    [System.Serializable]
    public class TimeTravelScene
    {
        public float targetYear;
        public string sceneName;
        public string spawnPointTag;
    }

    public TimeTravelScene[] timeTravelScenes;

    [Header("=== Time Travel Effect (For Lever Only) ===")]
    public float delayBeforeSceneLoad = 2f;
    public AudioSource timeTravelAudio;
    public ParticleSystem timeTravelEffect;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!useBoxTrigger || !other.CompareTag("Player")) return;

        SaveSceneAndSpawn(spawnPointTag);
        UnitySceneManager.LoadScene(sceneToLoad);
    }

    private void Update()
    {
        if (!useLever || hasTriggered) return;

        foreach (var travelScene in timeTravelScenes)
        {
            if (Mathf.Abs(leverValue - travelScene.targetYear) <= leverTolerance)
            {
                hasTriggered = true;
                StartCoroutine(TriggerTimeTravelSequence(travelScene));
                break;
            }
        }
    }

    public void ForceSceneCheck()
    {
        // DEPRECATED – not used anymore
    }

    public void SwitchToScene() // ✅ NEW direct switch
    {
        if (hasTriggered) return;
        hasTriggered = true;

        StartCoroutine(TriggerTimeTravelSequence(new TimeTravelScene
        {
            sceneName = sceneToLoad,
            spawnPointTag = spawnPointTag
        }));
    }

    private IEnumerator TriggerTimeTravelSequence(TimeTravelScene scene)
    {
        SaveSceneAndSpawn(scene.spawnPointTag);

        if (timeTravelAudio != null) timeTravelAudio.Play();
        if (timeTravelEffect != null) timeTravelEffect.Play();

        yield return new WaitForSeconds(delayBeforeSceneLoad);

        UnitySceneManager.LoadScene(scene.sceneName);
    }

    private void SaveSceneAndSpawn(string spawn)
    {
        PlayerPrefs.SetString("LastScene", UnitySceneManager.GetActiveScene().name);

        if (!string.IsNullOrEmpty(spawn))
        {
            PlayerPrefs.SetString("SpawnPoint", spawn);
        }
    }

    public bool HasTriggered() => hasTriggered;
}
