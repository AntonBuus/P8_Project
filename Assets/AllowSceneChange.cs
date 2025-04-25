using UnityEngine;

public class AllowSceneChange : MonoBehaviour
{
    public TTS_both_API ttsSystem; // Reference to the TTS system
    public LeverTimeDial leverTimeDial;
    


    void Start()
    {
        ttsSystem = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        leverTimeDial = GetComponent<LeverTimeDial>();

    }

    // Update is called once per frame
    void Update()
    {
        if (ttsSystem.isSecondAudioReady)
        {
            leverTimeDial.enabled = true;
        }
        else
        {
            leverTimeDial.enabled = false;
        }
    }
}
