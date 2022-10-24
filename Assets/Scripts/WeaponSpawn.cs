using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour {
    public GameObject pistolPrefab;
    public Transform weaponPos;

    private Weapon currentWeapon;
    private bool spawning = true;

    // Start is called before the first frame update
    void Start() {
        // Start by spawning a weapon
        SpawnWeapon(pistolPrefab);
    }

    private void SpawnWeapon(GameObject weapon) {
        currentWeapon = Instantiate(weapon, weaponPos.position, Quaternion.identity).transform.Find("model").GetComponent<Weapon>();
        spawning = false;

    }

    IEnumerator WaitAndSpawn(GameObject weapon) {
        // suspend execution for 10 seconds
        yield return new WaitForSeconds(10);
        SpawnWeapon(weapon);
    }

    private void Update() {
        // Check if the weapon has been taken
        if (!spawning && currentWeapon.GetCarrier() != null) {
            // Start a timer to spawn a new weapon
            currentWeapon = null;
            IEnumerator coroutine = WaitAndSpawn(pistolPrefab);
            StartCoroutine(coroutine);
            spawning = true;
        }

        if (currentWeapon != null) {
            // Make weapon start rotating
            currentWeapon.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * 40);
        }
    }
}
