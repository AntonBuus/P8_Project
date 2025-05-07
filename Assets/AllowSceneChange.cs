using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AllowSceneChange : MonoBehaviour
{
    public TTS_both_API ttsSystem; // Reference to the TTS system
    public BoxCollider handleCollider;
    public XRGrabInteractable XRGrabInteractable;
    


    void Start()
    {
        ttsSystem = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        handleCollider = GetComponent<BoxCollider>();
        XRGrabInteractable = GetComponent<XRGrabInteractable>();

    }

    // Update is called once per frame
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
