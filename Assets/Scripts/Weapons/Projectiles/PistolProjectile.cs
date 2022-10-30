using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolProjectile : MonoBehaviour {
    public Player shooter;

    private void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;
        
        Player p = collider.GetComponent<Player>();
        if (p != null && p != shooter) {
            // Bullet has hit a player, do damage
            p.Damage(10);
        }

        // Destroy the bullet
        Destroy(gameObject);
    }
}
