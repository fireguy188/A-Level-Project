using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour {
    public Transform grapplehook_loc;
    public bool grappling = false;

    // Update is called once per frame
    private void Update() {
        if (!grappling) {
            SetPositionRotation(grapplehook_loc.position, grapplehook_loc.rotation);
        }
    }

    public void SetPositionRotation(Vector3 newpos, Quaternion newrot) {
        transform.position = grapplehook_loc.position;
        transform.rotation = grapplehook_loc.rotation;
    }

    private void OnCollisionEnter(Collision collision) {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
    }
}
