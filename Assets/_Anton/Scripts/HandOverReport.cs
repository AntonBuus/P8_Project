using UnityEngine;

public class HandOverReport : MonoBehaviour
{
    public GameObject _hunReport;

    void Start()
    {
        _hunReport = GameObject.Find("HunReport");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "HunReport")
        {
            Debug.Log("HunReport handed!");
            _hunReport.transform.position = this.transform.position;
            _hunReport.transform.SetParent(this.transform);
        }
        Debug.Log("something hit here.");
    }
}
