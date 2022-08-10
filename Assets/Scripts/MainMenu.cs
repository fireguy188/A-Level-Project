using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour {  
   private static MainMenu _singleton;
   public static MainMenu Singleton {
      get => _singleton;
      private set {
            if (_singleton == null) {
               _singleton = value;
            } else if (_singleton != value) {
               Debug.Log($"{nameof(MainMenu)} instance already exists, destroying object!");
               Destroy(value);
            }
      }
   }

   [SerializeField] private TMP_InputField usernameField;
   [SerializeField] private TMP_InputField IPField;

   private void Awake() {
      Singleton = this;
   }

   public void StartClicked() {
      string IP = IPField.GetComponent<TMP_InputField>().text;
      if (string.IsNullOrEmpty(IP)) {
         // This player wants to be a host
         NetworkManager.Singleton.StartHost();
      } else {
         // This player wants to be a client
         NetworkManager.Singleton.JoinGame(IP);
      }
   }

   public void ExitClicked() {
      UnityEditor.EditorApplication.isPlaying = false;
      Application.Quit();
   }

   public void HelpClicked() {
      SceneManager.LoadScene("HelpMenu");
   }

   public string getUsername() {
      return usernameField.GetComponent<TMP_InputField>().text;
   }
}
