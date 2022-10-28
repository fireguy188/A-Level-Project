using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolProjectile : MonoBehaviour {
    private void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;

        Player p = collider.GetComponent<Player>();
        if (p != null) {
            // Bullet has hit a player, do damage
            Debug.Log("ow");
        }

        // Destroy the bullet
        Destroy(gameObject);
    }
}