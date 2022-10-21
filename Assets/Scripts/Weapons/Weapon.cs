using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    protected int ammo;
    protected Player carrier = null;

    public Weapon(int ammo) {
        this.ammo = ammo;
    }

    // When a player runs into the weapon, if it has not already been picked up
    // Make this player pick it up
    protected void OnCollisionEnter(Collision collision) {
        if (carrier == null && collision.collider.transform.GetComponent<Player>() != null) {
            carrier = collision.collider.transform.GetComponent<Player>();
            carrier.c_weapon = this;
            transform.parent = carrier.cam.transform;
            GetComponent<MeshCollider>().enabled = false;
        } 
    }

    // Update is called once per frame
    void Update() {
        if (carrier != null) {
            transform.position = carrier.gun_loc.position;
            transform.rotation = carrier.gun_loc.rotation;
        }
    }

}
