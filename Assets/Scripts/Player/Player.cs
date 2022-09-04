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
    public int jumpForce = 200;

    private void OnDestroy() {
        List.Remove(Id);
    }

    private void Update() {
        if (ingame && Id == NetworkManager.Singleton.Client.Id) {
            if (Input.GetKey(KeyCode.Space)) {
                GetComponent<Rigidbody>().AddForce(transform.up * jumpForce);
                SendJump();
            }

            SendRotation();
        }
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

    public void SendRotation() {
        // This message will be sent many times per second so the messages can be unreliable
        Message message = Message.Create(MessageSendMode.unreliable, MessageId.sendRotation);

        // The important part: sending the orientation of the players
        message.AddQuaternion(transform.rotation);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendJump() {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.sendJump);
        message.AddInt(jumpForce);
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

    public static void SendStart() {
        // Send the message that the game is starting to every client
        Message message = Message.Create(MessageSendMode.reliable, MessageId.start);
        NetworkManager.Singleton.Server.SendToAll(message);
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

    [MessageHandler((ushort)MessageId.usernameTaken)]
    private static void UsernameTaken(Message message) {
        if (message.GetBool()) {
            NetworkManager.Singleton.Client.Disconnect();
            foreach (Player player in Player.List.Values) {
                Destroy(player.gameObject);
            }
            Player.List.Clear();
            
            MainMenu.Singleton.DisplayPopup("Username already taken");
        } else {
            SceneManager.LoadScene("GameLobby");
        }
    }

    [MessageHandler((ushort)MessageId.start)]
    private static void ReceiveStart(Message message) {
        // The client has received info that the game has started
        SceneManager.LoadScene(NetworkManager.Singleton.GetChosenMap());

        // When the scene is loaded, the gameManager object in that level will handle
        // spawning in players
    }

    [MessageHandler((ushort)MessageId.sendRotation)]
    private static void ReceiveRotation(Message message) {
        ushort id = message.GetUShort();
        Quaternion rotation = message.GetQuaternion();

        Player.List[id].transform.rotation = rotation;
    }

    [MessageHandler((ushort)MessageId.sendJump)]
    private static void ReceiveJump(Message message) {
        ushort id = message.GetUShort();
        int jumpForce = message.GetInt();

        Player.List[id].GetComponent<Rigidbody>().AddForce(Player.List[id].transform.up * jumpForce);
    }

    /*
     * Server handling methods
     *
    */
    [MessageHandler((ushort)MessageId.addPlayer)]
    private static void AddPlayer(ushort fromClientId, Message message) {
        // When the server receives a username/Id from a client
        Message errorMsg;
        string toClientId = message.GetString();
        ushort _ = message.GetUShort();
        string username = message.GetString();

        // Check the username is not already in use
        foreach (Player player in List.Values) {
            if (player.username == username && player.Id != fromClientId) {
                // Tell this user their name has been taken
                errorMsg = Message.Create(MessageSendMode.reliable, MessageId.usernameTaken);
                errorMsg.AddBool(true);
                NetworkManager.Singleton.Server.Send(errorMsg, fromClientId);
                return;
            }
        }

        // Tell this user their name has not been taken
        errorMsg = Message.Create(MessageSendMode.reliable, MessageId.usernameTaken);
        errorMsg.AddBool(false);
        NetworkManager.Singleton.Server.Send(errorMsg, fromClientId);

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

    [MessageHandler((ushort)MessageId.sendRotation)]
    private static void ReceiveRotation(ushort fromClientId, Message message) {
        Quaternion rotation = message.GetQuaternion();

        Message msg = Message.Create(MessageSendMode.unreliable, MessageId.sendRotation);
        msg.AddUShort(fromClientId);
        msg.AddQuaternion(rotation);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.sendJump)]
    private static void ReceiveJump(ushort fromClientId, Message message) {
        int jumpForce = message.GetInt();

        Message msg = Message.Create(MessageSendMode.reliable, MessageId.sendJump);
        msg.AddUShort(fromClientId);
        msg.AddInt(jumpForce);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }
}
