using UnityEngine;

public class LeverConnectorLine : MonoBehaviour
{
    public Transform startPoint; // Pipboi base
    public Transform endPoint;   // Pull handle
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
    }

    void LateUpdate()
    {
        if (startPoint != null && endPoint != null)
        {
            line.SetPosition(0, startPoint.position);
            line.SetPosition(1, endPoint.position);
        }
    }
}
