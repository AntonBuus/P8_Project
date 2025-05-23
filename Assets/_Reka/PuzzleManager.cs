using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public OpenLid openLid;
    public List<GameObject> paperpieces = new List<GameObject>() { null, null, null, null };
    public string[] tagCombination;

    public void SetPieceAtIndex(int index, GameObject obj)
    {
        if (index >= 0 && index < paperpieces.Count)
        {
            paperpieces[index] = obj;
        }

        if (!paperpieces.Contains(null))
        {
            EvaluateOrder();
        }
    }

    public void EvaluateOrder()
    {
        for (int i = 0; i < tagCombination.Length; i++)
        {
            if (paperpieces[i] == null || !paperpieces[i].CompareTag(tagCombination[i]))
            {
                Debug.Log("Incorrect combination.");
                return;
            }
        }

        Debug.Log("Correct combination: OPEN");
        openLid.OpenLidNow(); 
    }
}
