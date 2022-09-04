using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private void Awake() {
        // Spawn in the players
        foreach (Player player in Player.List.Values) {
            // Get the player's spawn location
            Vector3 spawnLoc = GameObject.Find($"p{player.Id}spawn").transform.position;

            // Then get the RigidBody model of that player and set its spawn position
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            player.GetComponent<Rigidbody>().position = spawnLoc;
            player.ingame = true;

            // If this is this player's character, enable the camera
            if (player.Id == NetworkManager.Singleton.Client.Id) {
                player.gameObject.transform.Find("Camera").gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    private void Update() {
        
    }
}
