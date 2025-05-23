using UnityEngine;

public class LeverConnectorLine : MonoBehaviour
{
    public Transform startPoint; 
    public Transform endPoint;  
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
