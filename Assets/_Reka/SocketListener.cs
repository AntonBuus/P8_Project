using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketListener : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public int socketIndex;  // Set this manually per socket (0,1,2,3...)

    private SocketTagFunc socket;

    void Awake()
    {
        socket = GetComponent<SocketTagFunc>();
        socket.selectEntered.AddListener(OnObjectSnapped);
        socket.selectExited.AddListener(OnObjectRemoved);
    }

    void OnDestroy()
    {
        socket.selectEntered.RemoveListener(OnObjectSnapped);
        socket.selectExited.RemoveListener(OnObjectRemoved);
    }

    void OnObjectSnapped(SelectEnterEventArgs args)
    {
        puzzleManager.SetPieceAtIndex(socketIndex, args.interactableObject.transform.gameObject);
    }

    void OnObjectRemoved(SelectExitEventArgs args)
    {
        puzzleManager.SetPieceAtIndex(socketIndex, null);
    }
}
