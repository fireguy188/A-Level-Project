using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpMenu : MonoBehaviour
{
    public void ExitClicked() {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void ReturnClicked() {
        SceneManager.LoadScene("MainMenu");
    }
}
