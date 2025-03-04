using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace OpenAI
{
    public class SupervisorInitialCall : MonoBehaviour
    {
        [SerializeField] private InputField inputField;

        private OpenAIApi openai = new OpenAIApi(); // Creating an instance of the OpenAIApi class

        private List<ChatMessage> messages = new List<ChatMessage>(); // Initializing a list to store chat messages

        //[TextArea(3, 10)]
        //public string _initialPrompt = "Insert Text";

        public TMP_Text _initialPrompt;

        [SerializeField] private TextMeshProUGUI OutputText;


        public async void SendReply() // Async method to send a reply
        {
            var newMessage = new ChatMessage() // Creating a new chat message
            {
                Role = "user", // Setting the role of the message to "user"
                Content = inputField.text // Setting the content of the message to the text from the input field
            };
            
            if (messages.Count == 0) newMessage.Content = _initialPrompt.text + "\n" + inputField.text; // If it's the first message, prepend the prompt to the content
            messages.Add(newMessage); // Adding the new message to the list of messages

            inputField.text = ""; // Clearing the input field
       

            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest() // Sending a request to the OpenAI API to create a chat completion
            {
                Model = "gpt-3.5-turbo-0125", // Setting the model to use for the completion
                Messages = messages // Passing the list of messages to the request
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0) // Checking if the response contains choices
            {
                var message = completionResponse.Choices[0].Message; // Getting the first message from the response
                message.Content = message.Content.Trim(); // Trimming any whitespace from the message content

                messages.Add(message); // Adding the message to the list of messages
                //AppendMessage(message); // Appending the message to the scroll view
                OutputText.text = message.Content; // Displaying the message content in the output text
                


            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt."); // Logging a warning if no text was generated
            }
            
        }
    }
}

