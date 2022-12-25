using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour {
    public Transform grapplehook_loc;
    public GameObject player_prefab;
    public int current_collisions = 0;

    private Player player;

    private void Start() {
        player = player_prefab.GetComponent<Player>();
    }

    private void Update() {
        if (transform.parent != null) {
            transform.position = grapplehook_loc.position;
            transform.rotation = grapplehook_loc.rotation;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (transform.parent == null && collision.collider != player.player_collider) {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // Activate step 2 of grappling
            Vector3 hitpoint = collision.contacts[0].point;
            Vector3 grappleDirection = (hitpoint - player.transform.position);

            player.Grapple2(grappleDirection, player.getGrapplePlayerSpeed());
        }
    }
}
