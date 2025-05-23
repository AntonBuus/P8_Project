using UnityEngine;

public class QuickSceneChange : MonoBehaviour
{
    public string sceneName = "SceneName"; 
    public float delay = 2f; 

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
