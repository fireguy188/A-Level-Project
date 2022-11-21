using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

    private Canvas gameOverCanvas;
    private TMP_Text gameOverMessage;
    
    public bool gameOver = false;
    private bool displayedWinner = false;

    private void Awake() {
        gameOverCanvas = Instantiate(Resources.Load<Canvas>("Prefabs/GameOverCanvas"));
        gameOverMessage = gameOverCanvas.transform.Find("Message").GetComponent<TMP_Text>();
        gameOverCanvas.enabled = false;
        
        // Spawn in the players
        foreach (Player player in Player.List.Values) {
            // Get the player's spawn location
            Vector3 spawnLoc = GameObject.Find($"p{player.Id}spawn").transform.position;
            Quaternion spawnRot = GameObject.Find($"p{player.Id}spawn").transform.rotation;

            // Then get the RigidBody model of that player and set its spawn position
            player.model.velocity = Vector3.zero;
            player.model.transform.position = spawnLoc;
            player.model.transform.rotation = spawnRot;
            player.ingame = true;

            if (player.cam.GetComponent<Camera>() != null) {
                player.noWeaponText.transform.parent.parent.gameObject.SetActive(true);
            }

            // If this is this player's character, enable the camera
            if (player.Id == NetworkManager.Singleton.Client.Id) {
                player.gameObject.transform.Find("Camera").gameObject.SetActive(true);
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
        }
    }
}
