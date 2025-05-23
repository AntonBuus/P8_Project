using System.Collections;
using System.IO; 
using UnityEngine;
using UnityEngine.Networking; 
using System.Text;
using Newtonsoft.Json; 

public class TTS_test : MonoBehaviour
{

    
    private string ttsEndpoint = "https://api.openai.com/v1/audio/speech";

    void Start()
    {
        StartCoroutine(ConvertTextToSpeech("Hello, welcome to Unity with OpenAI Text-to-Speech!"));
    }

    IEnumerator ConvertTextToSpeech(string text)
    {
        var requestBody = new
        {
            model = "tts-1", 
            voice = "onyx",
            input = text
        };

        string jsonData = JsonConvert.SerializeObject(requestBody);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(ttsEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

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
