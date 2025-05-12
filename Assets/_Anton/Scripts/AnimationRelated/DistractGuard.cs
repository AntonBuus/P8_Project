using UnityEditor.Overlays;
using UnityEngine;


public class DistractGuard : MonoBehaviour
{
    public AudioClip StoneHitSound; // Assign your audio clip in the inspector
    AudioSource audioSource; // Assign your AudioSource in the inspector
    public GameObject SceneObjectToEnable;
    
    public GameObject _distractableGuard; // Assign your stone prefab in the inspector
    public TransformBetweenPoints _transformBetweenPoints;
    public Transform _newGuardPosition; // Assign your stone prefab in the inspector

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stone"))
        {
            // Assuming you have a method to play the audio
            // audioSource.PlayOneShot(StoneHitSound);
            
            RegularInvoke(); //Assign in the inspector what sounds to play
            // Destroy the stone after playing the sound
            Destroy(other.gameObject);
            // Enable the scene object 
            if (SceneObjectToEnable != null)
            {
                SceneObjectToEnable.SetActive(true);
            }

            _transformBetweenPoints.StartMoving(); // Start the movement of the guard

            // _distractableGuard.transform.position = new Vector3(_newGuardPosition.position.x, 
            // _distractableGuard.transform.position.y,_newGuardPosition.position.z); // Set the new position for the guard
        }
    }

    public UnityEngine.Events.UnityEvent onInvoke;
        private void RegularInvoke()
    {
        if (onInvoke != null)
        {
            onInvoke.Invoke();
        }
    }
}
