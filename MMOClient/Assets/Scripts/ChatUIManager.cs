using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class ChatUIManager : MonoBehaviour
{
    public PhoenixChatClient chatClient;
    public TMP_Text chatHistoryText;
    public ScrollRect scrollRect;
    public TMP_InputField inputField;
    public Button sendButton;
    public string playerName = "player1";
    public bool chatActive = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        if (!chatActive)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                chatActive = true;
                Cursor.lockState = CursorLockMode.None;
                inputField.ActivateInputField();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                chatActive = false;
                inputField.text = string.Empty;
                inputField.DeactivateInputField();
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                SendFromInput();
                chatActive = false;
                inputField.DeactivateInputField();
                Cursor.lockState = CursorLockMode.Locked;
            }
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

        // Deserialize into ChatPayload since PhoenixMessage now stores payload as object
        var payload = JsonConvert.DeserializeObject<ChatPayload>(msg.payload.ToString());
        if (payload == null)
            return;

        string line = $"[{payload.from}] {payload.text}";
        chatHistoryText.text += line + "\n";
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
