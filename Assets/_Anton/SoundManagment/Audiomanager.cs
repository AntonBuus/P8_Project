using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Creats the sound array
    public Sound[] soundsArray;


    //Singleton pattern
    public static AudioManager instance;

    void Awake()
    {
        //Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in soundsArray)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
        }


    }

    void Start()
    {
        Play("Theme");
        //Debug.Log("Start is working");
    }

    //To call:
    //FindObjectOfType<AudioManager>().Play("clip_name");
    //
    public void Play(string name)
    {
        Sound s = Array.Find(soundsArray, soundsArray => soundsArray.name == name);

        if (s == null)
        {
            Debug.LogWarning(name + " check spelling");
            return;
        }
        s.source.Play();
        Debug.Log("Playing sound: " + name);
    }




    public void DisableAudioSource(string audioSourceName)
    {
        // Find the sound with the specified name
        Sound s = Array.Find(soundsArray, sound => sound.name == audioSourceName);

        if (s != null)
        {
            if (s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }
}