using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

    private Canvas gameOverCanvas;
    private TMP_Text gameOverMessage;
    [SerializeField] private TMP_Text winsInfo;
    
    public bool gameOver = false;
    private bool displayedWinner = false;
    private Dictionary<ushort, int> wins = new Dictionary<ushort, int>();

    private void Awake() {
        gameOverCanvas = Instantiate(Resources.Load<Canvas>("Prefabs/GameOverCanvas"));
        gameOverMessage = gameOverCanvas.transform.Find("Message").GetComponent<TMP_Text>();
        gameOverCanvas.enabled = false;

        // Display initial win count of 0 for every player
        string winsInfoStr = "";
        foreach (Player p in Player.List.Values) {
            winsInfoStr += $"{p.username}: 0\n";
        }

        winsInfo.text = winsInfoStr;
        
        // Spawn in the players
        foreach (Player player in Player.List.Values) {
            wins[player.Id] = 0;

            // Get the player's spawn location
            Vector3 spawnLoc = GameObject.Find($"p{player.Id}spawn").transform.position;
            Quaternion spawnRot = GameObject.Find($"p{player.Id}spawn").transform.rotation;

            // Then get the RigidBody model of that player and set its spawn position
            player.model.velocity = Vector3.zero;
            player.model.transform.position = spawnLoc;
            player.model.transform.rotation = spawnRot;
            player.ingame = true;

            // If this is this player's character, enable the camera and the HUD
            if (player.Id == NetworkManager.Singleton.Client.Id) {
                player.gameObject.transform.Find("Camera").gameObject.SetActive(true);
                player.noWeaponText.transform.parent.parent.gameObject.SetActive(true);
            }
        }
    }

    private void Update() {
        Player lastAlive = null;
        foreach (Player player in Player.List.Values) {
            if (!player.isDead) {
                if (lastAlive == null) {
                    lastAlive = player;
                    gameOver = true;
                } else {
                    gameOver = false;
                }
            }
        }

        if (gameOver && !displayedWinner) {
            displayedWinner = true;
            
            // Display you won to the player who has won
            if (lastAlive.Id == NetworkManager.Singleton.Client.Id) {
                gameOverMessage.text = "You won!";
            } else {
                // Display the name of the person who won
                gameOverMessage.text = $"{lastAlive.username} won!";
            }

            // Replace the HUD with the winner message
            lastAlive.noWeaponText.transform.parent.parent.gameObject.SetActive(false);
            gameOverCanvas.enabled = true;

            wins[lastAlive.Id]++;
            // Display new win count for every player
            string winsInfoStr = "";

            foreach (Player p in Player.List.Values) {
                winsInfoStr += $"{p.username}: {wins[p.Id]}\n";
            }

            winsInfo.text = winsInfoStr;

            if (wins[lastAlive.Id] == 3) {
                // Finish the game
            } else {
                // New round
                Invoke("StartNewRound", 5);
            }
        }
    }

    private void StartNewRound() {
        displayedWinner = false;
        gameOverCanvas.enabled = false;

        ushort[] keyArray = new ushort[Player.List.Count];
        int i = 0;
        foreach (ushort id in Player.List.Keys) {
            keyArray[i] = id;
            i++;
        }

        // Remove every player from the game and add back a fresh player
        foreach (ushort id in keyArray) {
            string username = Player.List[id].username;
            Destroy(Player.List[id].grapple_hook);
            Destroy(Player.List[id].gameObject);
            Player.AddPlayer(id, username);
        }
        
        // Spawn in the players
        foreach (Player player in Player.List.Values) {
            // Get the player's spawn location
            Vector3 spawnLoc = GameObject.Find($"p{player.Id}spawn").transform.position;
            Quaternion spawnRot = GameObject.Find($"p{player.Id}spawn").transform.rotation;

            // Reset the player
            player.model.velocity = Vector3.zero;
            player.model.transform.position = spawnLoc;
            player.model.transform.rotation = spawnRot;
            player.ingame = true;

            // If this is this player's character, enable the camera and the HUD
            if (player.Id == NetworkManager.Singleton.Client.Id) {
                player.gameObject.transform.Find("Camera").gameObject.SetActive(true);
                player.noWeaponText.transform.parent.parent.gameObject.SetActive(true);
            }
        }
    }
}
