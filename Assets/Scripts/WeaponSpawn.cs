using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour {
    public GameObject pistolPrefab;
    public Transform weaponPos;

    private GameObject currentWeapon;

    // Start is called before the first frame update
    void Start() {
        // Start by spawning a weapon
        currentWeapon = Instantiate(pistolPrefab, weaponPos.position, Quaternion.identity);
    }


}
