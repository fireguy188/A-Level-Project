using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour {

    private Transform cam;

    private void Start() {
        foreach (Player p in Player.List.Values) {
            Camera pCamera = p.cam.GetComponent<Camera>();
            if (pCamera != null) {
                cam = pCamera.transform;
                break;
            }
        }
    }

    private void LateUpdate() {
        transform.LookAt(transform.position + cam.forward);
    }
}
