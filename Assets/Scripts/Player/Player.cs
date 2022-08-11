using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiptideNetworking;

public class Player : MonoBehaviour {
    public static Dictionary<ushort, Player> List = new Dictionary<ushort, Player>();

    public ushort Id;
    public string username;
    public bool ingame;

    private void OnDestroy() {
        List.Remove(Id);
    }

    public static void AddPlayer(ushort id, string username, bool alert_others = false) {
        // Creating a new player object and adding it to the list of players
        Player player;
        if (id == NetworkManager.Singleton.Client.Id) {
            player = Instantiate(NetworkManager.Singleton.localPlayerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        } else {
            player = Instantiate(NetworkManager.Singleton.playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        }
        
        player.Id = id;
        player.username = username;
        player.ingame = false;
        player.name = $"Player {id} ({username})";
        DontDestroyOnLoad(player);
        List.Add(id, player);
        if (SceneManager.GetActiveScene().name == "GameLobby") {
            GameLobbyMenu.Singleton.UpdateLobbyMenu();
        }

        if (alert_others) {
            // Telling the server what their username and id is
            // (This would be if the player has just joined)
            player.SendDetails("");
        }
    }

    /*
     * Client sending methods
     *
    */
    public void SendDetails(string toClientId) {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.addPlayer);
        message.AddString(toClientId);
        message.AddUShort(Id);
        message.AddString(username);
        NetworkManager.Singleton.Client.Send(message);
    }

    
    /*
     * Server sending methods
     *
    */
    public static void SendMap() {
        // Send the current map choice to every client
        Message message = Message.Create(MessageSendMode.reliable, MessageId.sendMap);
        message.AddString(NetworkManager.Singleton.GetChosenMap());
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void SendMap(ushort toClientId) {
        // Send the current map choice to chosen client
        Message message = Message.Create(MessageSendMode.reliable, MessageId.sendMap);
        message.AddString(NetworkManager.Singleton.GetChosenMap());
        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    /*
     * Client handling methods
     *
    */
    [MessageHandler((ushort)MessageId.addPlayer)]
    private static void AddPlayer(Message message) {
        // Remove the useless toClientId string from the message
        message.GetString();
        
        // When a client receives a username/Id from the server
        AddPlayer(message.GetUShort(), message.GetString());
    }

    [MessageHandler((ushort)MessageId.sendMap)]
    private static void ReceiveMap(Message message) {
        // When client receives a new chosen map from the server, update the chosenMap variable
        string mapName = message.GetString();
        NetworkManager.Singleton.SetChosenMap(mapName);
    }

    /*
     * Server handling methods
     *
    */
    [MessageHandler((ushort)MessageId.addPlayer)]
    private static void AddPlayer(ushort fromClientId, Message message) {
        // When the server receives a username/Id from a client

        string toClientId = message.GetString();

        if (toClientId == "") {
            // Go through every current player already connected
            foreach (ushort other_id in List.Keys) {
                // Send this new player's details to every other user on the server
                if (other_id != fromClientId) {
                    NetworkManager.Singleton.Server.Send(message, other_id);
                }
            }

            // Send them the chosen map as they are a new player
            SendMap(fromClientId);
        } else {
            NetworkManager.Singleton.Server.Send(message, ushort.Parse(toClientId));
        }
    }
}
