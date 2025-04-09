using UnityEngine;

public class FindGameObject : MonoBehaviour
{
    //This is to be inserted to any script that needs to influence or activate prompts
    [SerializeField] private AdjustablePrompts adjustablePrompts;
    void Start()
    {
        if (adjustablePrompts == null)
        {
            adjustablePrompts = GameObject.Find("AdjustablePrompts").GetComponent<AdjustablePrompts>();
        }
    }

}
