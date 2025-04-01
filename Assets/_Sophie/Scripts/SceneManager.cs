using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    [Header("=== Scene Trigger Settings ===")]
    [Tooltip("Enable if you want this object to trigger a scene change on player collision.")]
    public bool useBoxTrigger = true;

    [Tooltip("Name of the scene to load when the player enters the collider.")]
    public string sceneToLoad;

    [Tooltip("Spawn point tag used when entering the new scene.")]
    public string spawnPointTag;

    [Header("=== Lever-Based Time Travel Settings ===")]
    [Tooltip("Enable if this object listens for a lever to control time travel.")]
    public bool useLever = false;

    [Tooltip("Lever input value, controlled by an external script.")]
    public float leverValue;

    [Tooltip("Scene mappings based on lever value ranges.")]
    public SceneMapping[] timeTravelScenes;

    [System.Serializable]
    public class SceneMapping
    {
        public float minLeverValue;
        public float maxLeverValue;
        public string sceneName;
        public string spawnPointTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useBoxTrigger || !other.CompareTag("Player")) return;

        SaveSceneAndSpawn(spawnPointTag);
        UnitySceneManager.LoadScene(sceneToLoad);
    }

    private void Update()
    {
        if (!useLever) return;

        foreach (var mapping in timeTravelScenes)
        {
            if (leverValue >= mapping.minLeverValue && leverValue < mapping.maxLeverValue)
            {
                SaveSceneAndSpawn(mapping.spawnPointTag);
                UnitySceneManager.LoadScene(mapping.sceneName);
                enabled = false; // Prevent reloading repeatedly
                break;
            }
        }
    }

    private void SaveSceneAndSpawn(string spawn)
    {
        PlayerPrefs.SetString("LastScene", UnitySceneManager.GetActiveScene().name);

        if (!string.IsNullOrEmpty(spawn))
        {
            PlayerPrefs.SetString("SpawnPoint", spawn);
        }
    }
}

