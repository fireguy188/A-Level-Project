using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        
    }

    public void ContinueClicked() {
        transform.parent.GetComponent<Canvas>().enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitClicked() {
        NetworkManager.Singleton.LeaveGame();
    }
}
