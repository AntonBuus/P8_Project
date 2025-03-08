using System.Collections;
using System.IO; // For saving the MP3 file
using UnityEngine;
using UnityEngine.Networking; // For making API requests
using System.Text;
using Newtonsoft.Json; // To convert C# objects into JSON format
using TMPro;

public class TTS_both_API : MonoBehaviour
{
    public enum TTSProvider { PlayHT, OpenAI } // Enum to switch between Play.ht and OpenAI
    [SerializeField] private TTSProvider selectedTTSProvider = TTSProvider.OpenAI; // Dropdown in Inspector

    [SerializeField] private TextMeshProUGUI GeneratedInput; // The AI-generated text field
    public AudioSource audioSource; // The audio source assigned in Unity

    private string playHTUrl = "https://api.play.ht/api/v2/tts/stream"; // Play.ht API URL
    private string openAITtsUrl = "https://api.openai.com/v1/audio/speech"; // OpenAI TTS API URL

    public void StartVoice()
    {
        string textToConvert = GeneratedInput.text; // Get the generated text
        
        if (selectedTTSProvider == TTSProvider.PlayHT)
        {
            StartCoroutine(GeneratePlayHTSpeech(textToConvert));
        }
        else if (selectedTTSProvider == TTSProvider.OpenAI)
        {
            StartCoroutine(GenerateOpenAISpeech(textToConvert));
        }
    }

    // 📌 Play.ht API Integration
    IEnumerator GeneratePlayHTSpeech(string text)
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

        using (UnityWebRequest request = new UnityWebRequest(playHTUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.PlayHT_ApiKey);
            request.SetRequestHeader("X-User-Id", SuperSecretStuff.PlayHT_UserId);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                string filePath = SaveAudioFile(audioData, "playht_audio.mp3");
                StartCoroutine(PlayAudio(filePath));
            }
            else
            {
                Debug.LogError("Play.ht TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    // 📌 OpenAI API Integration
    IEnumerator GenerateOpenAISpeech(string text)
    {
        var requestBody = new
        {
            model = "tts-1",
            voice = "onyx",
            input = text
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(openAITtsUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.OPENAI_NAHRS_ApiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = request.downloadHandler.data;
                string filePath = SaveAudioFile(audioData, "openai_audio.mp3");
                StartCoroutine(PlayAudio(filePath));
            }
            else
            {
                Debug.LogError("OpenAI TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    // 📌 Save the MP3 File
    string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        Debug.Log("Audio saved to: " + filePath);
        return filePath;
    }

    // 📌 Play the MP3 File
    IEnumerator PlayAudio(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
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
