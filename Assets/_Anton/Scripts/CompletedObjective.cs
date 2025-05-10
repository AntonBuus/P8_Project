using UnityEngine;

public class CompletedObjective : MonoBehaviour
{
 private bool _hasplayedthisScene = false;
    void Start()
    {
        _hasplayedthisScene = false;
    }

    public UnityEngine.Events.UnityEvent onInvoke;


    public void InvokeOnceInThisScene()
    {
        if (_hasplayedthisScene == false)
        {
            RegularInvoke();
            _hasplayedthisScene = true;
            Debug.Log("invoked once in this scene");
        }
    }
    
    private void RegularInvoke()
    {
        if (onInvoke != null)
        {
            onInvoke.Invoke();
        }
    }
}
