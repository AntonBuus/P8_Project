using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenClipboard : MonoBehaviour
{
    public GameObject clipboard;

    public InputActionProperty showMenuButton;

    void Update()
    {
        if (showMenuButton.action.WasPressedThisFrame())
        {
            ActivateDeactivateClipboard();
            Debug.Log("Clipboard opened!");
        }
    }

    public void ActivateDeactivateClipboard()
    {
        clipboard.SetActive(!clipboard.activeSelf);
    }
}
