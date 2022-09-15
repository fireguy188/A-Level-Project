using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour {
    public Transform grapplehook_loc;

    // Update is called once per frame
    void Update() {
        SetPositionRotation(grapplehook_loc.position, grapplehook_loc.rotation);
    }

    public void SetPositionRotation(Vector3 newpos, Quaternion newrot) {
        transform.position = grapplehook_loc.position;
        transform.rotation = grapplehook_loc.rotation;
    }
}
