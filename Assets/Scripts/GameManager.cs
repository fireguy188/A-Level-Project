using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private void Awake() {
        // Spawn in the players
        foreach (Player player in Player.List.Values) {
            // Get the player's spawn location
            Vector3 spawnLoc = GameObject.Find($"p{player.Id}spawn").transform.position;
            Quaternion spawnRot = GameObject.Find($"p{player.Id}spawn").transform.rotation;

            // Then get the RigidBody model of that player and set its spawn position
            player.model.velocity = Vector3.zero;
            player.model.transform.position = spawnLoc;
            player.model.transform.rotation = spawnRot;

            // Also get the model of the grappling hook and set its spawn position
            // player.grapple_hook.transform.parent = player.transform.Find("grapplehook_loc");
            // player.grapple_hook.transform.position.Set(0, 0, 5);
            player.ingame = true;

            if (player.noWeaponText != null) {
                player.noWeaponText.transform.parent.parent.gameObject.SetActive(true);
            }

            // If this is this player's character, enable the camera
            if (player.Id == NetworkManager.Singleton.Client.Id) {
                player.gameObject.transform.Find("Camera").gameObject.SetActive(true);
            }
        }
    }
}
