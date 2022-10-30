using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiptideNetworking;

public class Player : MonoBehaviour {
    public static Dictionary<ushort, Player> List = new Dictionary<ushort, Player>();

    public ushort Id;
    public string username;
    public bool ingame;
    public Rigidbody model;
    public GameObject grapple_hook;
    public GameObject cam;
    public GameObject headTop;
    public LineRenderer lineRenderer;
    public Rigidbody grapple_hook_model;
    public GrappleHook grapple_hook_script;
    public CapsuleCollider player_collider;
    public Weapon c_weapon;
    public Transform gun_loc;
    public HealthBar healthBar;

    private int health = 100;
    private float jumpForce = 7f;
    private float grappleHookSpeed = 50f;
    private float grapplePlayerSpeed = 30f;
    private bool[] inputs = {false, false, false};
    private static Dictionary<ushort, float> jumpers = new Dictionary<ushort, float>();
    private static Dictionary<ushort, Tuple<Vector3, float>> startGrapplers = new Dictionary<ushort, Tuple<Vector3, float>>();

    public float getGrapplePlayerSpeed() {
        return this.grapplePlayerSpeed;
    }

    public void Damage(int dmg) {
        health -= dmg;
        
        if (health < 0) {
            health = 0;
        }
        
        if (healthBar != null) {
            healthBar.SetHealth(health);
        }
    }
    
    private void OnDestroy() {
        Destroy(grapple_hook);
        List.Remove(Id);
    }

    private void OnTriggerEnter(Collider other) {
        // Player will collide with one of the child parts of the grapple hook
        // Check if the grapple hook they collided into was their's
        if (other.gameObject.transform.parent != null && other.gameObject.transform.parent.gameObject == grapple_hook) {
            model.velocity = Vector3.zero;
            player_collider.isTrigger = false;
            model.constraints = RigidbodyConstraints.FreezePosition;

            // Cast the ray cast from the top of the player
            if (Physics.Raycast(headTop.transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 2f)) {
                UnGrapple();
            }
        }
    }

    private void Start() {
        grapple_hook_model = grapple_hook.GetComponent<Rigidbody>();
        grapple_hook_script = grapple_hook.GetComponent<GrappleHook>();
        player_collider = GetComponent<CapsuleCollider>();
    }

    private void Update() {
        if (model.velocity.x < 0.2) {
            model.velocity.Set(0, model.velocity.y, model.velocity.z);
        }

        if (model.velocity.z < 0.2) {
            model.velocity.Set(model.velocity.x, model.velocity.y, 0);
        }

        if (ingame && Id == NetworkManager.Singleton.Client.Id) {
            if (Input.GetKey(KeyCode.Space)) {
                inputs[0] = true;
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                inputs[1] = true;
            }

            if (Input.GetMouseButtonDown(0)) {
                inputs[2] = true;
            }

            SendModelDetails();
        }
    }

    private void FixedUpdate() {
        // Draw line for grappling hook
        if (grapple_hook_script.grappling) {
            lineRenderer.SetPosition(0, grapple_hook_script.grapplehook_loc.position);
            lineRenderer.SetPosition(1, grapple_hook_model.position);
        } else {
            // If they aren't grappling, make sure a line doesn't appear
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }

        // Handle own movements
        if (!grapple_hook_script.grappling && inputs[0] && Physics.Raycast(transform.position, -Vector3.up, GetComponent<Collider>().bounds.extents.y + 0.01f)) {
            model.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            SendJump();
        }

        inputs[0] = false;

        // If the player wants to start grappling
        if (inputs[1]) {
            if (!grapple_hook_script.grappling && grapple_hook_script.current_collisions == 0) {
                RaycastHit grapplePoint;
                Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out grapplePoint);

                Vector3 grappleDirection = (grapplePoint.point - grapple_hook_script.grapplehook_loc.position);
                Grapple1(grappleDirection, grappleHookSpeed);
                SendStartGrapple(grappleDirection, grappleHookSpeed);
            } else {
                // Stop grappling process
                UnGrapple();
                SendUnGrapple();
            }
        }

        inputs[1] = false;

        // If the player wants to shoot their gun
        if (inputs[2]) {
            if (c_weapon != null) {
                RaycastHit shootPoint;
                Ray ray = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out shootPoint);

                Vector3 shootDirection = (shootPoint.point - c_weapon.projectileSpawn.position);
                c_weapon.Shoot(shootDirection);
                SendShoot(shootDirection);
            }
        }

        inputs[2] = false;

        // Handle others movements
        // The jumpers dictionary stores player ids with the jumpForces they have
        foreach (ushort id in jumpers.Keys) {
            List[id].model.AddForce(Vector3.up * jumpers[id], ForceMode.Impulse);
        }

        jumpers.Clear();

        foreach (ushort id in startGrapplers.Keys) {
            Vector3 grappleDirection = startGrapplers[id].Item1;
            float grappleHookSpeed = startGrapplers[id].Item2;
            List[id].Grapple1(grappleDirection, grappleHookSpeed);
        }

        startGrapplers.Clear();

        // Send sync message every 20 ticks
        if (ingame && Id == NetworkManager.Singleton.Client.Id && NetworkManager.Singleton.tick % 20 == 0) {
            SendSync();
        }
    }

    // First step of grappling, shooting the grapple hook out
    public void Grapple1(Vector3 grappleDirection, float grappleHookSpeed) {
        grapple_hook_model.velocity = grappleDirection.normalized * grappleHookSpeed;
        grapple_hook_model.rotation = Quaternion.LookRotation(grappleDirection.normalized);
        grapple_hook_model.constraints = RigidbodyConstraints.FreezeRotation;
        grapple_hook_script.grappling = true;
        grapple_hook.transform.parent = null;
    }

    // Second step of grappling, player goes towards grapple hook
    public void Grapple2(Vector3 grappleDirection, float grapplePlayerSpeed) {
        model.velocity = grappleDirection.normalized * grapplePlayerSpeed;
        model.rotation = Quaternion.LookRotation(grappleDirection.normalized);
        model.useGravity = false;
        player_collider.isTrigger = true;
    }

    public void UnGrapple() {
        model.useGravity = true;
        player_collider.isTrigger = false;
        model.constraints = RigidbodyConstraints.None;
        
        grapple_hook_model.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        grapple_hook_script.grappling = false;
        grapple_hook.transform.parent = cam.transform;
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

    // Used for sending unimportant details, rotations and such and such
    public void SendModelDetails() {
        // This message will be sent many times per second so the messages can be unreliable
        Message message = Message.Create(MessageSendMode.unreliable, MessageId.sendModelDetails);

        // The important part: sending the orientation of the players
        message.AddQuaternion(transform.rotation);

        message.AddQuaternion(cam.transform.localRotation);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendJump() {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.sendJump);
        message.AddFloat(jumpForce);
        NetworkManager.Singleton.Client.Send(message);
    }

    // Used for sending important details
    public void SendSync() {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.sync);

        // Send player model details
        message.AddVector3(model.velocity);
        message.AddVector3(model.position);

        // Send grapple hook model details
        message.AddBool(model.useGravity);
        message.AddBool(grapple_hook_script.grappling);
        message.AddBool(player_collider.isTrigger);
        message.AddVector3(grapple_hook_model.velocity);
        message.AddVector3(grapple_hook_model.position);

        // Send gun details
        if (c_weapon != null) {
            message.AddString(c_weapon.GetWeaponName());
            message.AddInt(c_weapon.GetAmmo());
        } else {
            message.AddString("");
            message.AddInt(0);
        }
        message.AddInt(health);

        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendStartGrapple(Vector3 grappleDirection, float grappleHookSpeed) {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.startGrapple);
        message.AddVector3(grappleDirection);
        message.AddFloat(grappleHookSpeed);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendUnGrapple() {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.unGrapple);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendShoot(Vector3 shootDirection) {
        Message message = Message.Create(MessageSendMode.reliable, MessageId.shoot);
        message.AddVector3(shootDirection);
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

    [MessageHandler((ushort)MessageId.sendModelDetails)]
    private static void ReceiveModelDetails(Message message) {
        ushort id = message.GetUShort();
        Quaternion rotation = message.GetQuaternion();

        Quaternion cam_rotation = message.GetQuaternion();

        List[id].transform.rotation = rotation;
        List[id].cam.transform.localRotation = cam_rotation;
    }

    [MessageHandler((ushort)MessageId.sendJump)]
    private static void ReceiveJump(Message message) {
        ushort id = message.GetUShort();
        float jumpForce = message.GetFloat();

        jumpers[id] = jumpForce;
    }
    
    [MessageHandler((ushort)MessageId.sync)]
    private static void ReceiveSync(Message message) {
        ushort id = message.GetUShort();
        Vector3 vel = message.GetVector3();
        Vector3 pos = message.GetVector3();

        bool using_gravity = message.GetBool();
        bool grappling = message.GetBool();
        bool isTrigger = message.GetBool();
        Vector3 grapple_hook_vel = message.GetVector3();
        Vector3 grapple_hook_pos = message.GetVector3();

        string weaponName = message.GetString();
        int ammo = message.GetInt();
        int health = message.GetInt();

        List[id].model.velocity = vel;
        List[id].transform.position = pos;

        List[id].model.useGravity = using_gravity;
        List[id].player_collider.isTrigger = isTrigger;

        List[id].grapple_hook_script.grappling = grappling;

        if (grappling) {
            List[id].grapple_hook_model.velocity = grapple_hook_vel;
            List[id].grapple_hook_model.position = grapple_hook_pos;
        }

        // If the weapon this player is holding is wrong
        if (List[id].c_weapon == null || List[id].c_weapon.GetWeaponName() != weaponName || List[id].c_weapon.GetAmmo() != ammo) {
            if (List[id].c_weapon != null) {
                Destroy(List[id].c_weapon.gameObject);
            }

            if (weaponName == "pistol") {
                GameObject weapon = Instantiate(Resources.Load<GameObject>(WeaponSpawn.pistolPrefabPath), Vector3.zero, Quaternion.identity);
                List[id].c_weapon = weapon.GetComponent<Weapon>();
                weapon.transform.parent = List[id].cam.transform;
                weapon.GetComponent<MeshCollider>().enabled = false;
                List[id].c_weapon.SetCarrier(List[id]);
            }
        }

        List[id].health = health;
        List[id].healthBar.SetHealth(health);
    }
    
    [MessageHandler((ushort)MessageId.startGrapple)]
    private static void ReceiveStartGrapple(Message message) {
        ushort id = message.GetUShort();
        Vector3 grappleDirection = message.GetVector3();
        float grappleHookSpeed = message.GetFloat();

        startGrapplers[id] = Tuple.Create<Vector3, float>(grappleDirection, grappleHookSpeed);
    }

    [MessageHandler((ushort)MessageId.unGrapple)]
    private static void ReceiveUnGrapple(Message message) {
        ushort id = message.GetUShort();

        List[id].UnGrapple();
    }

    [MessageHandler((ushort)MessageId.shoot)]
    private static void ReceiveShoot(Message message) {
        ushort id = message.GetUShort();
        Vector3 shootDirection = message.GetVector3();

        List[id].c_weapon.Shoot(shootDirection);
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

    [MessageHandler((ushort)MessageId.sendModelDetails)]
    private static void ReceiveModelDetails(ushort fromClientId, Message message) {
        Quaternion rotation = message.GetQuaternion();
        Quaternion cam_rotation = message.GetQuaternion();

        Message msg = Message.Create(MessageSendMode.unreliable, MessageId.sendModelDetails);
        msg.AddUShort(fromClientId);
        msg.AddQuaternion(rotation);
        msg.AddQuaternion(cam_rotation);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.sendJump)]
    private static void ReceiveJump(ushort fromClientId, Message message) {
        float jumpForce = message.GetFloat();

        Message msg = Message.Create(MessageSendMode.reliable, MessageId.sendJump);
        msg.AddUShort(fromClientId);
        msg.AddFloat(jumpForce);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.sync)]
    private static void ReceiveSync(ushort fromClientId, Message message) {
        Vector3 vel = message.GetVector3();
        Vector3 pos = message.GetVector3();

        bool using_gravity = message.GetBool();
        bool grappling = message.GetBool();
        bool isTrigger = message.GetBool();
        Vector3 grapple_hook_vel = message.GetVector3();
        Vector3 grapple_hook_pos = message.GetVector3();

        string weaponName = message.GetString();
        int ammo = message.GetInt();
        int health = message.GetInt();

        Message msg = Message.Create(MessageSendMode.reliable, MessageId.sync);
        msg.AddUShort(fromClientId);
        msg.AddVector3(vel);
        msg.AddVector3(pos);

        msg.AddBool(using_gravity);
        msg.AddBool(grappling);
        msg.AddBool(isTrigger);
        msg.AddVector3(grapple_hook_vel);
        msg.AddVector3(grapple_hook_pos);

        msg.AddString(weaponName);
        msg.AddInt(ammo);
        msg.AddInt(health);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.startGrapple)]
    private static void ReceiveStartGrapple(ushort fromClientId, Message message) {
        Vector3 grappleDirection = message.GetVector3();
        float grappleHookSpeed = message.GetFloat();

        Message msg = Message.Create(MessageSendMode.reliable, MessageId.startGrapple);
        msg.AddUShort(fromClientId);
        msg.AddVector3(grappleDirection);
        msg.AddFloat(grappleHookSpeed);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.unGrapple)]
    private static void ReceiveUnGrapple(ushort fromClientId, Message message) {
        Message msg = Message.Create(MessageSendMode.reliable, MessageId.unGrapple);
        msg.AddUShort(fromClientId);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }

    [MessageHandler((ushort)MessageId.shoot)]
    private static void ReceiveShoot(ushort fromClientId, Message message) {
        Vector3 shootDirection = message.GetVector3();

        Message msg = Message.Create(MessageSendMode.reliable, MessageId.shoot);
        msg.AddUShort(fromClientId);
        msg.AddVector3(shootDirection);

        foreach (ushort id in List.Keys) {
            if (id != fromClientId) {
                NetworkManager.Singleton.Server.Send(msg, id);
            }
        }
    }
}
