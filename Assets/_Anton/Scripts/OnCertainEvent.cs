using UnityEngine;
using System;
using System.Collections.Generic;

public class OnCertainEvent : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent onActivate;
    public UnityEngine.Events.UnityEvent onStart;
    public UnityEngine.Events.UnityEvent onEnable;
    public UnityEngine.Events.UnityEvent onUpdate;
    public UnityEngine.Events.UnityEvent onDestroy;


    public void InvokeStart()
    {
        if (onStart != null)
        {
            onStart.Invoke();
        }
    }
    public void OnEnable()
    {
        if (onEnable != null)
        {
            onEnable.Invoke();
        }
    }
    public void InvokeActivate()
    {
        if (onActivate != null)
        {
            onActivate.Invoke();
        }
    }
    public void InvokeUpdate()
    {
        if (onUpdate != null)
        {
            onUpdate.Invoke();
        }
    }

    public void InvokeDestroy()
    {
        if (onDestroy != null)
        {
            onDestroy.Invoke();
        }
    }


}
