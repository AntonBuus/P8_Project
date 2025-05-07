using UnityEngine;

public class FlipPage : MonoBehaviour
{
    public bool _openPage
    { get; set; }

    private float _rotationSpeed = 200f;
    private float _targetAngle = 170f;
    private float _closedAngle = 0f;

    void Update()
    {
        float targetAngle = _openPage ? _targetAngle : _closedAngle;
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, targetAngle, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
}
