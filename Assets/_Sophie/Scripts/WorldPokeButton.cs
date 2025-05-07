using UnityEngine;

public class WorldPokeButton : MonoBehaviour
{
    public float pressDistance = 0.005f;       // How far the button moves when pressed
    public float returnSpeed = 0.01f;          // How fast it returns to its original position
    public Vector3 pressDirection = Vector3.down; // Local press direction (e.g., Vector3.down = -Y)
    
    [HideInInspector]
    public bool isPressed = false;

    private Vector3 startPos;
    private Vector3 targetPos;

    void Start()
    {
        // Save the starting position in world space
        startPos = transform.position;

        // Calculate the world-space target position based on local direction
        targetPos = startPos + transform.TransformDirection(pressDirection.normalized * pressDistance);
    }

    void Update()
    {
        if (isPressed)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, returnSpeed);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, returnSpeed);
        }
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
