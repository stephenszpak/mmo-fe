using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    public PhoenixChatClient chatClient;
    public Text chatHistoryText;
    public ScrollRect scrollRect;
    public InputField inputField;
    public Button sendButton;
    public string playerName = "player1";

    void Start()
    {
        if (chatClient != null)
        {
            chatClient.playerName = playerName;
            chatClient.OnChatMessage += OnChatMessage;
        }
        if (sendButton != null)
            sendButton.onClick.AddListener(SendFromInput);
        if (inputField != null)
            inputField.onEndEdit.AddListener(OnEndEdit);
    }

    void OnDestroy()
    {
        if (chatClient != null)
            chatClient.OnChatMessage -= OnChatMessage;
    }

    void OnEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            SendFromInput();
    }

    void SendFromInput()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;

        string to = chatClient != null ? chatClient.globalTopic : "chat:global";
        string message = inputField.text;

        if (message.StartsWith("/w "))
        {
            var parts = message.Split(new char[] { ' ' }, 3);
            if (parts.Length >= 3)
            {
                to = "chat:whisper:" + parts[1];
                message = parts[2];
            }
        }

        chatClient?.SendChat(to, message);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    void OnChatMessage(PhoenixMessage msg)
    {
        if (msg.payload == null)
            return;

        string line = $"[{msg.payload.from}] {msg.payload.text}";
        chatHistoryText.text += line + "\n";
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
