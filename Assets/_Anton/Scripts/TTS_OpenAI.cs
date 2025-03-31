using System.Collections;
using System.IO; // To save the received MP3 file.
using UnityEngine;
using UnityEngine.Networking; // For making API requests.
using System.Text;
using Newtonsoft.Json; // To convert C# objects into JSON format.
using TMPro;
using System.Collections.Generic;

public class TTS_OpenAI : MonoBehaviour
{
    
    private string ttsEndpoint = "https://api.openai.com/v1/audio/speech";

    [SerializeField] private TextMeshProUGUI GeneratedInput;

    public enum Voice
    {
        echo,
        fable,
        onyx,
        nova,
        shimmer
    }
    [SerializeField] private Voice selectedVoice;

    void Start()
    {
        StartCoroutine(ConvertTextToSpeech("Hello, I am an AI assistant. How can I help you today?"));
    }
    void OpenAIStartVoice()
    {
        StartCoroutine(ConvertTextToSpeech(GeneratedInput.text));
    }

    IEnumerator ConvertTextToSpeech(string text)
    {
        // Create JSON request body with model, voice and input.
        var requestBody = new
        {
             // Use "tts-1-hd" for higher quality
            model = "tts-1", // Model for text-to-speech
            voice = selectedVoice.ToString(), // Other voices: echo, fable, onyx, nova, shimmer
            input = text
        };

        string jsonData = JsonConvert.SerializeObject(requestBody);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // ending the HTTP POST Request to OpenAi.
        using (UnityWebRequest request = new UnityWebRequest(ttsEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.OPENAI_NAHRS_ApiKey);

            yield return request.SendWebRequest();

            //  Handling API Response: Checks if API request was successful. Saves the received MP3 file.
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                string filePath = SaveAudioFile(audioData, "tts_audio.mp3");
                StartCoroutine(PlayAudio(filePath));
            }
            else
            {
                Debug.LogError("OpenAI TTS Error: " + request.error);
            }
        }
    }

    // Stores the received MP3 file on disk.
    string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        Debug.Log("Audio saved to: " + filePath);
        return filePath;
    }

    // Loads and plays the audio file.
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
