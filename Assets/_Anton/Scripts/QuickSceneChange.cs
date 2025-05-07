using UnityEngine;

public class QuickSceneChange : MonoBehaviour
{
    public string sceneName = "SceneName"; // Replace with your scene name
    public float delay = 2f; // Delay before changing the scene

    private void Start()
    {
        // Invoke("ChangeSceneQuick", delay);
    }

    public void ChangeSceneQuick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public void ChangeSceneByIndex(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}
