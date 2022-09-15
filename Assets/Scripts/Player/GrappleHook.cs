using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleHook : MonoBehaviour {
    public Transform grapplehook_loc;
    public Camera cam;

    // Update is called once per frame
    void Update() {
        transform.position = grapplehook_loc.position;
        //transform.localRotation = cam.transform.localRotation;
    }
}
