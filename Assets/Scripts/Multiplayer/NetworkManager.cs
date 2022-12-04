using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

internal enum MessageId : ushort {
    addPlayer = 1,
    sendMap,
    usernameTaken,
    start,
    sendModelDetails,
    sendJump,
    sync,
    startGrapple,
    unGrapple,
    shoot
}

public class NetworkManager : MonoBehaviour {
    private static NetworkManager _singleton;
    public static NetworkManager Singleton {
        get => _singleton;
        private set {
            if (_singleton == null) {
                _singleton = value;
            } else if (_singleton != value) {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value.gameObject);
            }
        }
    }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxPlayers;
    private string connectedIP;
    private string chosenMap;
    private string popupMsg;

    public int tick = 0;
    public GameObject playerPrefab;
    public GameObject localPlayerPrefab;

    internal Server Server { get; private set; }
    internal Client Client { get; private set; }

    private void Awake() {
        Singleton = this;
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public string GetConnectedIp() {
        return connectedIP;
    }

    public string GetChosenMap() {
        return chosenMap;
    }

    public void SetChosenMap(string mapName) {
        chosenMap = mapName;
        if (GameLobbyMenu.Singleton != null) {
            GameLobbyMenu.Singleton.SetMapName(mapName);
        }
    }

    private void Start() {
        Application.targetFrameRate = 60;
        
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server { AllowAutoMessageRelay = true };

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientConnected += PlayerJoined;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }
    
    private void FixedUpdate() {
        if (Server.IsRunning) {
            Server.Tick();
        }
        
        Client.Tick();
        tick++;
    }

    private void OnApplicationQuit() {
        Server.Stop();
        Client.Disconnect();
    }
    
    internal void StartHost() {
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        connectedIP = externalIpString;
        chosenMap = "Swinging Start";
        Server.Start(port, maxPlayers);
        Client.Connect($"127.0.0.1:{port}");
        Player.SendMap();
    }

    internal void JoinGame(string ipString) {
        connectedIP = ipString;
        Client.Connect($"{ipString}:{port}");
    }

    internal void LeaveGame() {
        Client.Disconnect();
        Server.Stop();
        ReturnToMainMenu();
    }

    private void DidConnect(object sender, EventArgs e) {
        Player.AddPlayer(Client.Id, MainMenu.Singleton.getUsername(), true);
    }

    private void FailedToConnect(object sender, EventArgs e) {
        MainMenu.Singleton.DisplayPopup("Unable to connect to specified IP");
    }

    private void PlayerJoined(object sender, ClientConnectedEventArgs e) {
        // When a player joins, send my username and id to them
        Player.List[Client.Id].SendDetails(e.Id.ToString());
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
        // Player may have been disconnected without being added to list of players
        if (Player.List.ContainsKey(e.Id)) {
            Destroy(Player.List[e.Id].grapple_hook);
            Destroy(Player.List[e.Id].gameObject);
            Player.List.Remove(e.Id);
            if (SceneManager.GetActiveScene().name == "GameLobby") {
                GameLobbyMenu.Singleton.UpdateLobbyMenu();
            }
        }
    }

    private void DidDisconnect(object sender, EventArgs e) {
        popupMsg = "You were disconnected from the server";
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu() {
        foreach (Player player in Player.List.Values) {
            Destroy(player.grapple_hook);
            Destroy(player.gameObject);
        }
        Player.List.Clear();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainMenu" && !String.IsNullOrEmpty(popupMsg)) {
            MainMenu.Singleton.DisplayPopup(popupMsg);
            popupMsg = "";
        }
    }
}
