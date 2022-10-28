using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    protected int ammo;
    protected Player carrier = null;
    [SerializeField] protected GameObject projectilePrefab;

    public Transform projectileSpawn;

    public Weapon(int ammo) {
        this.ammo = ammo;
    }

    public Player GetCarrier() {
        return carrier;
    }

    // When a player runs into the weapon, if it has not already been picked up
    // Make this player pick it up
    protected void OnCollisionEnter(Collision collision) {
        if (carrier == null && collision.collider.transform.GetComponent<Player>() != null) {
            // If the player already has a weapon
            if (collision.collider.transform.GetComponent<Player>().c_weapon != null) {
                return;
            }
            
            carrier = collision.collider.transform.GetComponent<Player>();
            carrier.c_weapon = this;
            transform.parent = carrier.cam.transform;
            GetComponent<MeshCollider>().enabled = false;
        } 
    }

    // Update is called once per frame
    private void Update() {
        if (carrier != null) {
            transform.position = carrier.gun_loc.position;
            transform.rotation = carrier.gun_loc.rotation;
        }
    }

    public virtual void Shoot(Vector3 shootDirection) {
    }

}
