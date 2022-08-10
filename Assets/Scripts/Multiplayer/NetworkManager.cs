using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

internal enum MessageId : ushort {
    addPlayer = 1,
    sendMap
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

    public GameObject playerPrefab;
    public GameObject localPlayerPrefab;

    internal Server Server { get; private set; }
    internal Client Client { get; private set; }

    private void Awake() {
        Singleton = this;
        DontDestroyOnLoad(this);
    }

    public string GetConnectedIp() {
        return connectedIP;
    }

    public string GetChosenMap() {
        return chosenMap;
    }

    public void SetChosenMap(string mapName) {
        chosenMap = mapName;
        GameLobbyMenu.Singleton.SetMapName(mapName);
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
        Server.Stop();
        Client.Disconnect();
        ReturnToMainMenu();
    }

    private void DidConnect(object sender, EventArgs e) {
        Player.AddPlayer(Client.Id, MainMenu.Singleton.getUsername(), true);
        SceneManager.LoadScene("GameLobby");
    }

    private void FailedToConnect(object sender, EventArgs e) {
        SceneManager.LoadScene("MainMenu");
    }

    private void PlayerJoined(object sender, ClientConnectedEventArgs e) {
        // When a player joins, send my username and id to them
        Player.List[Client.Id].SendDetails(e.Id.ToString());
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) {
        Destroy(Player.List[e.Id].gameObject);
        Player.List.Remove(e.Id);
        if (SceneManager.GetActiveScene().name == "GameLobby") {
            GameLobbyMenu.Singleton.UpdatePlayerCount();
        }
    }

    private void DidDisconnect(object sender, EventArgs e) {
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu() {
        foreach (Player player in Player.List.Values) {
            Destroy(player.gameObject);
        }
        Player.List.Clear();

        SceneManager.LoadScene("MainMenu");
    }
}
