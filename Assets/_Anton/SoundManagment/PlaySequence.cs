using UnityEngine;
using System.Collections;

public class PlaySequence : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip firstClip;
    public AudioClip secondClip;

    private void Start()
    {
        StartCoroutine(PlayClipsInSequence());
    }

    private IEnumerator PlayClipsInSequence()
    {
        audioSource.clip = firstClip;
        audioSource.Play();
        yield return new WaitForSeconds(firstClip.length);

        audioSource.clip = secondClip;
        audioSource.Play();
    }
}
