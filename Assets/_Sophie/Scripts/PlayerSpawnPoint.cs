using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform positionEnter;
    public Transform positionEnterAgain;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        string spawnPoint = PlayerPrefs.GetString("SpawnPoint", "Enter");

        switch (spawnPoint)
        {
            case "Enter":
                if (positionEnter != null)
                {
                    player.transform.position = positionEnter.position;
                    player.transform.rotation = positionEnter.rotation;
                }
                break;

            case "OutOfTent":
                if (positionEnterAgain != null)
                {
                    player.transform.position = positionEnterAgain.position;
                    player.transform.rotation = positionEnterAgain.rotation;
                }
                break;

            default:
                Debug.LogWarning($"Unknown SpawnPoint: {spawnPoint}");
                break;
        }
    }
}