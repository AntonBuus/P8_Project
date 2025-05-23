using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class PlayerPositionManager : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform player; 

    [System.Serializable]
    public class SpawnPoint
    {
        public string fromScene;
        public Transform spawnLocation;
    }

    [Header("Spawn Points Based on Last Scene")]
    public SpawnPoint[] spawnPoints;

    [Header("Fallback Spawn Point")]
    public Transform fallbackSpawn;

    void Start()
    {
        string lastScene = PlayerPrefs.GetString("LastScene", "").Trim();
        string currentScene = UnitySceneManager.GetActiveScene().name;

        Debug.Log($"[PlayerPositionManager] Current Scene: {currentScene}, From Scene: {lastScene}");

        Transform chosenSpawn = GetSpawnPointFrom(lastScene);

        if (chosenSpawn != null)
        {
            MovePlayerTo(chosenSpawn);
            Debug.Log($"[PlayerPositionManager] Spawned at: {chosenSpawn.name}");
        }
        else if (fallbackSpawn != null)
        {
            MovePlayerTo(fallbackSpawn);
            Debug.LogWarning("[PlayerPositionManager] Using fallback spawn point!");
        }
        else
        {
            Debug.LogError("[PlayerPositionManager] No spawn point found or fallback assigned!");
        }
    }

    private Transform GetSpawnPointFrom(string fromScene)
    {
        foreach (var point in spawnPoints)
        {
            if (point.fromScene.Trim() == fromScene)
                return point.spawnLocation;
        }
        return null;
    }

    private void MovePlayerTo(Transform target)
    {
        if (player != null && target != null)
        {
            player.position = target.position;
            player.rotation = target.rotation;
        }
        else
        {
            Debug.LogError("[PlayerPositionManager] Player or target position is missing.");
        }
    }

    public static void SaveAndLoad(string toScene)
    {
        PlayerPrefs.SetString("LastScene", UnitySceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        UnitySceneManager.LoadScene(toScene);
    }
}
