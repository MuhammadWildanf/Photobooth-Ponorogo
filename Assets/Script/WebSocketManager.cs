using UnityEngine;
using NativeWebSocket;
using System.Text;
using System.Collections;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager Instance;
    public GameObject SelectGender;
    public GameObject Home;

    private WebSocket ws;
    public static string ActiveUser = null;

    // private string wsUrl = "ws://localhost:3002/ws/booth?booth_id=photobooth";
    private string wsUrl = "wss://scmdigitalday2025.com/ws/booth?booth_id=photobooth";

   void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
}


    async void Start()
    {
        ConnectWebSocket();
    }

    async void ConnectWebSocket()
    {
        ws = new WebSocket(wsUrl);

        ws.OnOpen += () =>
        {
            Debug.Log("WS Connected");
        };

        ws.OnError += (e) =>
        {
            Debug.LogError("WS Error: " + e);
        };

        ws.OnClose += (e) =>
        {
            Debug.LogWarning("WS Closed: " + e);
            StartCoroutine(TryReconnect());
        };

        ws.OnMessage += (bytes) =>
        {
            string json = Encoding.UTF8.GetString(bytes);
            Debug.Log("WS Message: " + json);

            WSMessage data = JsonUtility.FromJson<WSMessage>(json);

            string evt = !string.IsNullOrEmpty(data.eventName) ? data.eventName : data.@event;

            if (evt == "userLinked")
            {
                StartSession(data.user_id);
                Home.SetActive(false);
                SelectGender.SetActive(true);
            }

            if (evt == "sessionEnd")
            {
                EndSession();
            }
        };

        await ws.Connect();
    }

   IEnumerator TryReconnect()
{
    yield return new WaitForSeconds(2);

    if (ws != null && ws.State != WebSocketState.Closed)
        ws.Close();

    ConnectWebSocket();
}

    void Update()
    {
        ws?.DispatchMessageQueue();
    }

    async void OnApplicationQuit()
    {
        await ws.Close();
    }

    // ===========================
    // SESSION CONTROL
    // ===========================
    public void StartSession(string userId)
    {
        if (ActiveUser != null)
        {
            Debug.LogWarning("Booth sedang dipakai user: " + ActiveUser);
            return;
        }

        ActiveUser = userId;

        Debug.Log("📌 SESSION STARTED for user: " + userId);

        // TODO: tampilkan UI masuk
    }

    public void EndSession()
    {
        if (ActiveUser == null) return;

        Debug.Log("📌 SESSION ENDED for user: " + ActiveUser);

        Send(new WSOutgoing("sessionEnd", ActiveUser));

        ActiveUser = null;
    }

    // ===========================
    // SEND TO SERVER
    // ===========================
    public async void Send(object payload)
    {
        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("WS NOT READY, message ignored");
            return;
        }

        string json = JsonUtility.ToJson(payload);
        await ws.SendText(json);
    }


    public void end()
    {

    }

    public void retake()
    {

    }


    // ===========================
    // JSON STRUCT
    // ===========================
    [System.Serializable]
    public class WSMessage
    {
        public string eventName;   // beberapa server pakai eventName
        public string @event;      // beberapa server pakai event
        public string user_id;
        public string boothId;
    }

    [System.Serializable]
    public class WSOutgoing
    {
        public string eventName;
        public string user_id;

        public WSOutgoing(string evt, string id)
        {
            eventName = evt;
            user_id = id;
        }
    }
}
