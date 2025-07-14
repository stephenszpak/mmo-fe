using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUIManager : MonoBehaviour
{
    public PhoenixChatClient chatClient;
    public TMP_Text chatHistoryText;
    public ScrollRect scrollRect;
    public TMP_InputField inputField;
    public Button sendButton;
    public string playerName = "player1";

    void Start()
    {

        Debug.Log("✅ ChatUIManager STARTED");
        if (chatClient != null)
        {
            chatClient.playerName = playerName;
            chatClient.OnChatMessage += OnChatMessage;
        }
        if (sendButton != null)
            sendButton.onClick.AddListener(SendFromInput);
    }

    void OnDestroy()
    {
        if (chatClient != null)
            chatClient.OnChatMessage -= OnChatMessage;
    }


    void Update()
    {
        if (inputField != null && inputField.isFocused &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Debug.Log("⏎ Enter detected while input is focused");
            SendFromInput();
        }
    }

    void SendFromInput()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;

        Debug.Log("\uD83D\uDCE4 SendFromInput called");
        Debug.Log($"\u27A1\uFE0F Message typed: {inputField.text}");

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
