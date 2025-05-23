using System.Collections;
using UnityEngine;
using UnityEngine.Networking; 
using System.Text;
using Newtonsoft.Json; 
using System;
using System.IO; 
using TMPro;

public class Supervisor_TTS_playht : MonoBehaviour
{
    private string apiUrl = "https://api.play.ht/api/v2/tts/stream"; 


    public AudioSource audioSource; 

    [SerializeField] private TextMeshProUGUI GeneratedInput;

    public void StartVoice()
    { 
        StartCoroutine(GenerateSpeech(GeneratedInput.text)); 
    }
    IEnumerator GenerateSpeech(string text)
    {
        var requestBody = new
        {
            text = text,
            voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
            output_format = "mp3",
            voice_engine = "Play3.0-mini",
            speed = 1.0
        };


        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("TTS request successful! Processing response...");
                byte[] audioData = request.downloadHandler.data;

                if (audioData.Length > 0)
                {
                string filePath = SaveAudioFile(audioData, "tts_audio.mp3");
                    StartCoroutine(PlayAudio(filePath));
                }
                else
                {
                    Debug.LogError("Received empty audio data!");
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                Debug.LogError("API key used" + SuperSecretStuff.PlayHT_ApiKey);
            }
        }
    }

        string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        Debug.Log("Audio saved to: " + filePath);
        return filePath;
    }

    IEnumerator PlayAudio(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();
                Debug.Log("Playing audio...");
            }
            else
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
        }
    }
}