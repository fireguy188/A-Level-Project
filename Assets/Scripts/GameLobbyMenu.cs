using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameLobbyMenu : MonoBehaviour {
    private static GameLobbyMenu _singleton;
    public static GameLobbyMenu Singleton {
        get => _singleton;
        private set {
                if (_singleton == null) {
                    _singleton = value;
                } else if (_singleton != value) {
                    Debug.Log($"{nameof(GameLobbyMenu)} instance already exists, destroying object!");
                    Destroy(value);
                }
        }
    }

    [SerializeField] private TMP_Text playerCount;
    [SerializeField] private TMP_Text playerNames;
    [SerializeField] private TMP_Text IP;
    [SerializeField] private TMP_Text MapName;
    [SerializeField] private Button startBtn;

    private void Awake() {
        Singleton = this;
        IP.SetText($"Host IP: {NetworkManager.Singleton.GetConnectedIp()}");
        MapName.SetText($"Map Name: {NetworkManager.Singleton.GetChosenMap()}");
        UpdatePlayerCount();
    }

    public void SetMapName(string mapName) {
        MapName.SetText($"Map Name: {mapName}");
    }

    public void LeaveClicked() {
        NetworkManager.Singleton.LeaveGame();
    }

    public void UpdatePlayerCount() {
        if (Player.List.Count == 1) {
            playerCount.SetText("1 Player");
        } else {
            playerCount.SetText($"{Player.List.Count} Players");
        }
    }
}
