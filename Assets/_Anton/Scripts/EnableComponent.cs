using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnableComponent : MonoBehaviour
{
    public GameObject _targetComponent;

    public InputActionProperty _toggleButton;

    void Update()
    {
        if (_toggleButton.action.WasPressedThisFrame())
        {
            ActivateDeactivateClipboard();
            Debug.Log("Clipboard opened!");
        }
    }

    public void ActivateDeactivateClipboard()
    {
        _targetComponent.SetActive(!_targetComponent.activeSelf);
    }
}
