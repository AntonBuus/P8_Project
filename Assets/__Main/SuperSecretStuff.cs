using System.Xml;
using UnityEngine;

public class SuperSecretStuff : MonoBehaviour
{
    public static string PlayHT_ApiKey; // PlayHT API Key
    public static string PlayHT_UserId; // PlayHT User ID
    public static string OPENAI_NAHRS_ApiKey; // OpenAI NAHRS API Key

    private void Start()
    {
        LoadApiKeysFromXml("Assets/__Main/KeyCollection.xml");
    }

    private void LoadApiKeysFromXml(string filePath)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        XmlNodeList apiKeys = xmlDoc.GetElementsByTagName("ApiKey");
        foreach (XmlNode apiKeyNode in apiKeys)
        {
            string name = apiKeyNode.Attributes["name"].Value;
            string key = apiKeyNode["Key"].InnerText;

            switch (name)
            {
                case "PlayHT_API_Key":
                    PlayHT_ApiKey = key;
                    break;
                case "PLAYHT_User_ID":
                    PlayHT_UserId = key;
                    break;
                case "NAHRS_API_Key":
                    OPENAI_NAHRS_ApiKey = key;
                    break;
            }
        }

        Debug.Log("API Keys Loaded Successfully!");
        Debug.Log($"PlayHT API Key: {PlayHT_ApiKey}");
        Debug.Log($"PlayHT User ID: {PlayHT_UserId}");
        Debug.Log($"OpenAI NAHRS API Key: {OPENAI_NAHRS_ApiKey}");
    }
}