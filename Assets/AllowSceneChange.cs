using UnityEngine;

public class AllowSceneChange : MonoBehaviour
{
    public TTS_both_API ttsSystem; // Reference to the TTS system
    public BoxCollider handleCollider;
    


    void Start()
    {
        ttsSystem = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        handleCollider = GetComponent<BoxCollider>();

    }

    // Update is called once per frame
    void Update()
    {
        if (ttsSystem.isSecondAudioReady)
        {
            handleCollider.enabled = true;
        }
        else
        {
            handleCollider.enabled = false;
        }
    }
}
