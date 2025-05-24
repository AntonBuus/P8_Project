using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking; 
using System.Text;
using Newtonsoft.Json; 
using TMPro;

public class TTS_both_API : MonoBehaviour
{
    public enum TTSProvider { PlayHT, OpenAI, No_Speech } 
    [SerializeField] public TTSProvider selectedTTSProvider = TTSProvider.OpenAI; 

    [Header("TTS Settings")]
    [SerializeField] private TextMeshProUGUI GeneratedInput;
    public AudioSource audioSource; 

    [Header("Play.ht Controls")]
    [Range(0.1f, 5f)] public float speed = 1.0f; 
    [Range(0.1f, 2f)] public float temperature = 1.0f; 

    [Tooltip("Select an emotion for the speech (Only for Play.ht)")]
    public string[] emotionOptions = { "female_happy", "female_sad", "female_angry", "female_fearful", "female_disgust", "female_surprised", "male_happy", "male_sad", "male_angry", "male_fearful", "male_disgust", "male_surprised" };
    public int selectedEmotionIndex = 0;

    private string playHTUrl = "https://api.play.ht/api/v2/tts/stream";
    private string openAITtsUrl = "https://api.openai.com/v1/audio/speech"; 

    public string usableFilePath;
    public bool isAudioReady = false;
    public bool isSecondAudioReady = false; 
    public bool isThirdAudioReady = false; 
    public string _openaiSelectedVoice = "onyx"; 
    public string _playHTSelectedVoice = "s3://voice-cloning-zero-shot/d99d35e6-e625-4fa4-925a-d65172d358e1/adriansaad/manifest.json"; 
    public void StartVoice()
    {
        string textToConvert = GeneratedInput.text;
        
        if (selectedTTSProvider == TTSProvider.PlayHT)
        {
            StartCoroutine(GeneratePlayHTSpeech(textToConvert));
            //    StartCoroutine(GeneratePlayHTSpeech("Hello I am bob. I like to do programming and mess with my sanity."));
        }
        else if (selectedTTSProvider == TTSProvider.OpenAI)
        {
            StartCoroutine(GenerateOpenAISpeech(textToConvert));
        }
        else if (selectedTTSProvider == TTSProvider.No_Speech)
        {
            Debug.Log("No speech selected");
            isAudioReady = true; 
        }

    }

    IEnumerator GeneratePlayHTSpeech(string text)
    {
        var requestBody = new
        {
            text = text,
            voice = _playHTSelectedVoice, 
            output_format = "mp3",
            voice_engine = "Play3.0-mini",
            speed = speed,
            temperature = temperature, 
            emotion = emotionOptions[selectedEmotionIndex]
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
                string filePath = SaveAudioFile(audioData, "playht_audio_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp3");
                Debug.Log("Audio ready: " + filePath);
                usableFilePath = filePath;

                if (isAudioReady)
                {
                    if (isSecondAudioReady)
                    {
                        isThirdAudioReady = true; 
                        Debug.Log("Third audio is ready");
                    }
                    isSecondAudioReady = true; 
                    Debug.Log("Second audio is ready: ");
                    
                }
                isAudioReady = true; 
                
            }
            else
            {
                Debug.LogError("Play.ht TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }


    IEnumerator GenerateOpenAISpeech(string text)
    {
        var requestBody = new
        {
            model = "tts-1",
            voice = _openaiSelectedVoice, 
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
                string filePath = SaveAudioFile(audioData, "openai_audio_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp3");
                Debug.Log("Audio ready: " + filePath);
                usableFilePath = filePath;
                if (isAudioReady)
                {
                    if (isSecondAudioReady)
                    {
                        isThirdAudioReady = true; 
                        Debug.Log("Third audio is ready");

                        
                    }
                    isSecondAudioReady = true; 
                    Debug.Log("Second audio is ready: ");
                    
                }

                isAudioReady = true;

            }
            else
            {
                Debug.LogError("OpenAI TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }


    string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        return filePath;
        
    }

    public void InitializePlayAudio(string filePath)
    {
        StartCoroutine(PlayAudio(filePath));
    }
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
