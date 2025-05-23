using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class AllowObjectGrab : MonoBehaviour
{
    public TTS_both_API ttsSystem; 
    public BoxCollider _objectCollider;
    public XRGrabInteractable XRGrabInteractable;
    


    void Start()
    {
        ttsSystem = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        _objectCollider = GetComponent<BoxCollider>();
        XRGrabInteractable = GetComponent<XRGrabInteractable>();

    }
    void OnEnable()
    {
        Debug.Log("AllowObjectGrab script enabled");
    }

    void Update()
    {
        if (ttsSystem.isThirdAudioReady)
        {
            _objectCollider.enabled = true;
            XRGrabInteractable.enabled = true;

        }
        else
        {
            _objectCollider.enabled = false;
            XRGrabInteractable.enabled = false;
        }
    }
}
