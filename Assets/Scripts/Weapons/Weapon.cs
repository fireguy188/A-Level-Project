using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    protected int ammo;
    protected Player carrier = null;
    protected string weaponName;
    [SerializeField] protected Sprite weaponSprite;
    [SerializeField] protected GameObject projectilePrefab;

    public Transform projectileSpawn;

    public Weapon(int ammo, string weaponName) {
        this.ammo = ammo;
        this.weaponName = weaponName;
    }

    public Player GetCarrier() {
        return carrier;
    }

    public void SetCarrier(Player carrier) {
        this.carrier = carrier;
    }

    public int GetAmmo() {
        return ammo;
    }

    public string GetWeaponName() {
        return weaponName;
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

            // Update player's HUD
            if (carrier.noWeaponText != null) {
                carrier.weaponInfo.sprite = weaponSprite;
                carrier.ammoInfo.text = "16/16";

                carrier.noWeaponText.enabled = false;
                carrier.weaponInfo.enabled = true;
                carrier.ammoInfo.enabled = true;
            }
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
