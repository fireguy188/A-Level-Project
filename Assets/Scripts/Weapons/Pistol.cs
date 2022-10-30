using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Weapon {
    private float speed = 200;

    public Pistol() : base(16, "pistol") {
    }

    public Pistol(int ammo) : base(ammo, "pistol") {
    }

    public override void Shoot(Vector3 shootDirection) {
        Rigidbody projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation).transform.Find("model").GetComponent<Rigidbody>();
        projectile.velocity = shootDirection.normalized * speed;
        projectile.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
