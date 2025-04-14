using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public bool _openDoor
    { get; set; }
    TTS_both_API _registerAudio;
    private void Start()
    {
        _registerAudio = GameObject.Find("TTS API").GetComponent<TTS_both_API>();
        if (_registerAudio == null)
        {
            Debug.LogError("TTS_both_API component not found on the GameObject.");
        }
    }

    private float _rotationSpeed = 150f;
    private float _targetAngle = 170f;
    public float _closedAngle = 0f;

    void Update()
    {
        float targetAngle;
        if (_registerAudio.isAudioReady)
        {
            targetAngle = _targetAngle;
        }
        else
        {
            targetAngle = _closedAngle;
        }
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, targetAngle, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
}
