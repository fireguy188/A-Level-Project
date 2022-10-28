using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon {
    private float speed = 200;

    public Pistol() : base(16) {
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    public override void Shoot(Vector3 shootDirection) {
        Rigidbody projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation).transform.Find("model").GetComponent<Rigidbody>();
        projectile.velocity = shootDirection.normalized * speed;
        projectile.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
