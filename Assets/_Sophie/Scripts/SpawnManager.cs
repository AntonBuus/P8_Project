// Original Code for Scenemanagemnt logic, but line 22 - 60 was created by chatGBT

using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SpawnManager : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOrigin;

    [Header("Spawn Points")]
    public Transform defaultSpawnPoint;
    public Transform exitSpawnPoint;

    [Header("Scene Names")]
    public string fromSceneToTent = "Testscene_Hun_tent_1";
    public string currentSceneName = "Testscene_Hun_1";

    void Start()
    {
#if UNITY_EDITOR
        if (UnitySceneManager.GetActiveScene().name == currentSceneName)
        {
            Debug.Log("[SpawnManager] Editor Play Mode detected - clearing LastScene...");
            PlayerPrefs.DeleteKey("LastScene");
        }
#endif

        string activeScene = UnitySceneManager.GetActiveScene().name;
        string lastScene = PlayerPrefs.GetString("LastScene", "");

        Debug.Log($"[SpawnManager] Active Scene: {activeScene}");
        Debug.Log($"[SpawnManager] Last Scene: {lastScene}");

        if (activeScene != currentSceneName)
        {
            Debug.LogWarning("[SpawnManager] Not in the target scene. Skipping spawn logic.");
            return;
        }

        if (lastScene == fromSceneToTent && exitSpawnPoint != null)
        {
            xrOrigin.position = exitSpawnPoint.position;
            xrOrigin.rotation = exitSpawnPoint.rotation;
            Debug.Log("[SpawnManager] Spawned at EXIT spawn point.");
        }
        else if (defaultSpawnPoint != null)
        {
            xrOrigin.position = defaultSpawnPoint.position;
            xrOrigin.rotation = defaultSpawnPoint.rotation;
            Debug.Log("[SpawnManager] Spawned at DEFAULT spawn point.");
        }
        else
        {
            Debug.LogWarning("[SpawnManager] No spawn point was assigned!");
        }
    }
}
