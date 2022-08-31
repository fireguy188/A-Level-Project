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
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private TMP_Text startBtnText;
    [SerializeField] private Button startBtn;

    private void Awake() {
        Singleton = this;
        IP.SetText($"Host IP: {NetworkManager.Singleton.GetConnectedIp()}");
        mapName.SetText($"Map Name: {NetworkManager.Singleton.GetChosenMap()}");

        // Determine what the start button should say
        if (NetworkManager.Singleton.Client.Id == 1) {
            // They are the host
            startBtnText.SetText("Start");
        } else {
            // They are the client
            startBtnText.SetText("Waiting For Host");
            startBtn.interactable = false;
        }

        UpdateLobbyMenu();
    }

    public void SetMapName(string newMapName) {
        mapName.SetText($"Map Name: {newMapName}");
    }

    public void LeaveClicked() {
        NetworkManager.Singleton.LeaveGame();
    }

    public void UpdateLobbyMenu() {
        // Update the player count
        if (Player.List.Count == 1) {
            playerCount.SetText("1 Player");
        } else {
            playerCount.SetText($"{Player.List.Count} Players");
        }

        // Update the list of usernames
        string[] usernames = new string[Player.List.Count];
        int index = 0;
        foreach (Player player in Player.List.Values) {
            usernames[index] = player.username;
            if (player.Id == 1) {
                usernames[index] += " (host)";
            }
            index++;
        }

        playerNames.SetText(string.Join("\n", usernames));

        // Update start button interactability for host
        if (NetworkManager.Singleton.Client.Id == 1) {
            if (Player.List.Count == 1) {
                startBtn.interactable = false;
            } else {
                startBtn.interactable = true;
            }
        }
    }

    public void StartClicked() {
        Player.SendStart();
    }
}
