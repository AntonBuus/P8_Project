using UnityEngine;

public class ButtonPressMover : MonoBehaviour
{
    public Transform pressTarget;         // Where the button should move when pressed
    public Transform defaultPosition;     // Optional: Original position
    public float moveSpeed = 10f;

    private bool isPressed = false;

    private void Start()
    {
        if (defaultPosition == null)
            defaultPosition = new GameObject("DefaultPosition").transform;

        defaultPosition.position = transform.position;
        defaultPosition.rotation = transform.rotation;
    }

    void Update()
    {
        if (isPressed)
            transform.position = Vector3.Lerp(transform.position, pressTarget.position, Time.deltaTime * moveSpeed);
        else
            transform.position = Vector3.Lerp(transform.position, defaultPosition.position, Time.deltaTime * moveSpeed);
    }

    public void PressDown()
    {
        isPressed = true;
    }

    public void Release()
    {
        isPressed = false;
    }
}
