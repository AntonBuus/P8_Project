// using System.Collections;
// using System.IO; // For saving the MP3 file
// using UnityEngine;
// using UnityEngine.Networking; // For making API requests
// using System.Text;
// using Newtonsoft.Json; // To convert C# objects into JSON format
// using TMPro;

// public class TTS_both_API : MonoBehaviour
// {
//     public enum TTSProvider { PlayHT, OpenAI } // Enum to switch between Play.ht and OpenAI
//     [SerializeField] private TTSProvider selectedTTSProvider = TTSProvider.OpenAI; // Dropdown in Inspector

//     [SerializeField] private TextMeshProUGUI GeneratedInput; // The AI-generated text field
//     public AudioSource audioSource; // The audio source assigned in Unity

//     private string playHTUrl = "https://api.play.ht/api/v2/tts/stream"; // Play.ht API URL
//     private string openAITtsUrl = "https://api.openai.com/v1/audio/speech"; // OpenAI TTS API URL

//     public void StartVoice()
//     {
//         string textToConvert = GeneratedInput.text; // Get the generated text
        
//         if (selectedTTSProvider == TTSProvider.PlayHT)
//         {
//             StartCoroutine(GeneratePlayHTSpeech(textToConvert));
//         }
//         else if (selectedTTSProvider == TTSProvider.OpenAI)
//         {
//             StartCoroutine(GenerateOpenAISpeech(textToConvert));
//         }
//     }

//     // 📌 Play.ht API Integration
//     IEnumerator GeneratePlayHTSpeech(string text)
//     {
//         var requestBody = new
//         {
//             text = text,
//             voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
//             output_format = "mp3",
//             voice_engine = "Play3.0-mini",
//             speed = 1.0
//         };

//         string jsonPayload = JsonConvert.SerializeObject(requestBody);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

//         using (UnityWebRequest request = new UnityWebRequest(playHTUrl, "POST"))
//         {
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.PlayHT_ApiKey);
//             request.SetRequestHeader("X-User-Id", SuperSecretStuff.PlayHT_UserId);

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 byte[] audioData = request.downloadHandler.data;
//                 string filePath = SaveAudioFile(audioData, "playht_audio.mp3");
//                 StartCoroutine(PlayAudio(filePath));
//             }
//             else
//             {
//                 Debug.LogError("Play.ht TTS Error: " + request.error);
//                 Debug.LogError("Response: " + request.downloadHandler.text);
//             }
//         }
//     }

//     // 📌 OpenAI API Integration
//     IEnumerator GenerateOpenAISpeech(string text)
//     {
//         var requestBody = new
//         {
//             model = "tts-1",
//             voice = "onyx",
//             input = text
//         };

//         string jsonPayload = JsonConvert.SerializeObject(requestBody);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

//         using (UnityWebRequest request = new UnityWebRequest(openAITtsUrl, "POST"))
//         {
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.OPENAI_NAHRS_ApiKey);

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 byte[] audioData = request.downloadHandler.data;
//                 string filePath = SaveAudioFile(audioData, "openai_audio.mp3");
//                 StartCoroutine(PlayAudio(filePath));
//             }
//             else
//             {
//                 Debug.LogError("OpenAI TTS Error: " + request.error);
//                 Debug.LogError("Response: " + request.downloadHandler.text);
//             }
//         }
//     }

//     // 📌 Save the MP3 File
//     string SaveAudioFile(byte[] audioData, string fileName)
//     {
//         string filePath = Path.Combine(Application.persistentDataPath, fileName);
//         File.WriteAllBytes(filePath, audioData);
//         Debug.Log("Audio saved to: " + filePath);
//         return filePath;
//     }

//     // 📌 Play the MP3 File
//     IEnumerator PlayAudio(string filePath)
//     {
//         using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
//         {
//             yield return www.SendWebRequest();

//             if (www.result == UnityWebRequest.Result.Success)
//             {
//                 AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
//                 audioSource.clip = audioClip;
//                 audioSource.Play();
//                 Debug.Log("Playing audio...");
//             }
//             else
//             {
//                 Debug.LogError("Error loading audio: " + www.error);
//             }
//         }
//     }
// }

using System.Collections;
using System.IO; // For saving the MP3 file
using UnityEngine;
using UnityEngine.Networking; // For making API requests
using System.Text;
using Newtonsoft.Json; // To convert C# objects into JSON format
using TMPro;

public class TTS_both_API : MonoBehaviour
{
    public enum TTSProvider { PlayHT, OpenAI, No_Speech } // Enum to switch between Play.ht and OpenAI
    [SerializeField] public TTSProvider selectedTTSProvider = TTSProvider.OpenAI; // Dropdown in Inspector

    [Header("TTS Settings")]
    [SerializeField] private TextMeshProUGUI GeneratedInput; // The AI-generated text field
    public AudioSource audioSource; // The audio source assigned in Unity

    [Header("Play.ht Controls")]
    [Range(0.1f, 5f)] public float speed = 1.0f; // Speech speed
    [Range(0.1f, 2f)] public float temperature = 1.0f; // Randomness in speech
    // [SerializeField] private string emotion = "neutral"; // Emotion selection

    [Tooltip("Select an emotion for the speech (Only for Play.ht)")]
    public string[] emotionOptions = { "female_happy", "female_sad", "female_angry", "female_fearful", "female_disgust", "female_surprised", "male_happy", "male_sad", "male_angry", "male_fearful", "male_disgust", "male_surprised" };
    public int selectedEmotionIndex = 0; // Index for dropdown

    private string playHTUrl = "https://api.play.ht/api/v2/tts/stream"; // Play.ht API URL
    private string openAITtsUrl = "https://api.openai.com/v1/audio/speech"; // OpenAI TTS API URL

    public string usableFilePath;
    public bool isAudioReady = false; // Flag to check if audio is ready
    public bool isSecondAudioReady = false; // Flag to check if second audio is ready
    public bool isThirdAudioReady = false; // Flag to check if third audio is ready
    public string _openaiSelectedVoice = "onyx"; // Default voice for OpenAI
    public string _playHTSelectedVoice = "s3://voice-cloning-zero-shot/d99d35e6-e625-4fa4-925a-d65172d358e1/adriansaad/manifest.json"; // Default voice for Play.ht
    public void StartVoice()
    {
        string textToConvert = GeneratedInput.text; // Get the generated text
        
        if (selectedTTSProvider == TTSProvider.PlayHT)
        {
            // StartCoroutine(GeneratePlayHTSpeech(textToConvert));
               StartCoroutine(GeneratePlayHTSpeech("Hello I am bob. I like to do programming and mess with my sanity."));
        }
        else if (selectedTTSProvider == TTSProvider.OpenAI)
        {
            StartCoroutine(GenerateOpenAISpeech(textToConvert));
        }
        else if (selectedTTSProvider == TTSProvider.No_Speech)
        {
            Debug.Log("No speech selected");
            isAudioReady = true; // Set the bool true so door can open
        }

    }

    // Play.ht API Integration
    IEnumerator GeneratePlayHTSpeech(string text)
    {
        var requestBody = new
        {
            text = text,
            voice = _playHTSelectedVoice, // Uses the selected voice
            output_format = "mp3",
            voice_engine = "Play3.0-mini",
            speed = speed, // Uses the adjustable Inspector value
            temperature = temperature, // Uses the adjustable Inspector value
            // emotion = emotion // Uses the selected emotion
            emotion = emotionOptions[selectedEmotionIndex] // Uses dropdown selection
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
                // string filePath = SaveAudioFile(audioData, "playht_audio.mp3");
                string filePath = SaveAudioFile(audioData, "playht_audio_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp3");
                // StartCoroutine(PlayAudio(filePath));
                Debug.Log("Audio ready: " + filePath);
                usableFilePath = filePath;

                if (isAudioReady)
                {
                    if (isSecondAudioReady)
                    {
                        isThirdAudioReady = true; // Set the bool true so door can open
                        Debug.Log("Third audio is ready");
                        // You can add any additional logic here if needed
                    }
                    isSecondAudioReady = true; // Set the bool true so door can open
                    Debug.Log("Second audio is ready: ");
                    
                }
                isAudioReady = true; // Set the bool true so door can open
                
            }
            else
            {
                Debug.LogError("Play.ht TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    // OpenAI API Integration
    IEnumerator GenerateOpenAISpeech(string text)
    {
        var requestBody = new
        {
            model = "tts-1",
            voice = _openaiSelectedVoice, // you can also choose alloy, ballad, coral, echo, fable, onyx, nova, sage, shimmer
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
                // StartCoroutine(PlayAudio(filePath));
                Debug.Log("Audio ready: " + filePath);
                usableFilePath = filePath;
                if (isAudioReady)
                {
                    if (isSecondAudioReady)
                    {
                        isThirdAudioReady = true; // Set the bool true so door can open
                        Debug.Log("Third audio is ready");
                        // You can add any additional logic here if needed
                    }
                    isSecondAudioReady = true; // Set the bool true so door can open
                    Debug.Log("Second audio is ready: ");
                    
                }

                isAudioReady = true; // Set the bool true so door can open

            }
            else
            {
                Debug.LogError("OpenAI TTS Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    // Save the MP3 File
    string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        // Debug.Log("Audio saved to: " + filePath);
        return filePath;
        
    }

    // Play the MP3 File
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
