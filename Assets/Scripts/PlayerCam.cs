using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour {
    public float sensX;
    public float sensY;

    private float xRotation;
    private float yRotation;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = transform.parent.rotation.x * 180;
        yRotation = transform.parent.rotation.y * 180;
    }

    private void Update() {
        if (!Cursor.visible) {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.parent.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }
}
