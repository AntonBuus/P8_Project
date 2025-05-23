using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AllowSceneChange : MonoBehaviour
{
    public TTS_both_API ttsSystem; 
    public BoxCollider handleCollider;
    public XRGrabInteractable XRGrabInteractable;
    


    void Start()
    {
        ttsSystem = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        handleCollider = GetComponent<BoxCollider>();
        XRGrabInteractable = GetComponent<XRGrabInteractable>();

    }

    void Update()
    {
        if (ttsSystem.isSecondAudioReady)
        {
            handleCollider.enabled = true;
            XRGrabInteractable.enabled = true;

        }
        else
        {
            handleCollider.enabled = false;
            XRGrabInteractable.enabled = false;
        }
    }
}
