using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // For making API requests.
using System.Text;
using Newtonsoft.Json; // To convert C# objects into JSON format.
using System;
using System.IO; // To save the received MP3 file.
using TMPro;

public class Supervisor_TTS_playht : MonoBehaviour
{
    private string apiUrl = "https://api.play.ht/api/v2/tts/stream";  // Play.ht Streaming API URL.

    // Declares an AudioSource to play the generated speech.
    public AudioSource audioSource;  // Assigned this in Unity Inspector.

    // Sending the Text-to-Speech Request. Automatically starts the TTS process when the Unity scene begins. 
    // Right now it is using a premade text.
    // void Start()
    // {
    //     StartCoroutine(GenerateSpeech("Today’s top story? A daring intervention in Ancient Egypt where our time agent successfully retrieved… an iPad. That’s right, folks, a piece of 21st-century tech somehow found its way into the land of pharaohs, pyramids, and hieroglyphs! But did this high-tech tablet disrupt the course of history, or did our agent manage to set things right? Let's dive in."));
    // }

    // Public = Can be seen by everyone and it appeares in the inspector.
    // SerializeField = Can be seen in the inspector but is not public.
    [SerializeField] private TextMeshProUGUI GeneratedInput; // AI-generated text field

    public void StartVoice()
    { 
        StartCoroutine(GenerateSpeech(GeneratedInput.text)); // Convert AI-generated text to speech
    }
    IEnumerator GenerateSpeech(string text)
    {
        // Creating and Sending the API Request with text, voice, output format, specify voice engine and speed.
        var requestBody = new
        {
            text = text,
            voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
            output_format = "mp3",
            voice_engine = "Play3.0-mini", // Use "Play3.0-mini" for low-latency streaming
            speed = 1.0
        };

        // Converts the request into JSON format and encodes it in UTF-8.
        string jsonPayload = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        //  Sending the HTTP POST Request: Adds required headers of Authorization, X-User-Id, Content-Type.
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "Bearer " + SuperSecretStuff.PlayHT_ApiKey);
            //request.SetRequestHeader("X-User-Id", SuperSecretStuff.PlayHT_UserId);

            yield return request.SendWebRequest();

            // Handling API Response. If the request was successful, the script proceeds to process the audio data.
            // Checks if the API call was successful. -> Saves the received MP3 audio file. -> If there's an error, logs the response.
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("TTS request successful! Processing response...");

                // Get the raw audio data from the request. It is stored as a byte array, which will be used to create an audio file.
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

        // Saves the received audio data as an MP3 file in Unity’s persistent storage.
        string SaveAudioFile(byte[] audioData, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        Debug.Log("Audio saved to: " + filePath);
        return filePath;
    }

    // Plays the mp3 file using Unity’s AudioSource.
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