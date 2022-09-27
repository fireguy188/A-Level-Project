using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour {
    public Transform grapplehook_loc;
    public bool grappling = false;
    public GameObject player_prefab;
    public int current_collisions = 0;

    // Update is called once per frame
    private void Update() {
        if (!grappling) {
            transform.position = grapplehook_loc.position;
            transform.rotation = grapplehook_loc.rotation;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        current_collisions++;
        if (grappling) {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // Activate step 2 of grappling
            Player player = player_prefab.GetComponent<Player>();

            Vector3 hitpoint = collision.contacts[0].point;
            Vector3 grappleDirection = (hitpoint - player.transform.position);

            player.Grapple2(grappleDirection, player.getGrapplePlayerSpeed());
        }
    }

    private void OnCollisionExit(Collision collision) {
        current_collisions--;
    }
}
