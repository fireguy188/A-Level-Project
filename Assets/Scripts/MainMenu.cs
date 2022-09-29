using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
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
   [SerializeField] private Canvas canvas;
   [SerializeField] public GameObject popup;

   private void Awake() {
      Singleton = this;
   }

   public void OkClicked() {
      // Make the popup inactive
      popup.SetActive(false);

      // Make all inputs and buttons interactable again
      canvas.GetComponent<CanvasGroup>().interactable = true;
   }

   public void DisplayPopup(string message) {
      // Make all inputs and buttons greyed out
      canvas.GetComponent<CanvasGroup>().interactable = false;

      // Display the popup
      popup.SetActive(true);
      Transform errorText = popup.transform.Find("ErrorText");
      errorText.GetComponent<TextMeshProUGUI>().SetText(message);
   }

   public void StartClicked() {
      string IP = IPField.GetComponent<TMP_InputField>().text;
      string username = usernameField.GetComponent<TMP_InputField>().text.Trim();
      usernameField.GetComponent<TMP_InputField>().text = username;

      // Make sure user has not left username blank
      if (string.IsNullOrEmpty(username)) {
         DisplayPopup("You have not entered a username");
         return;
      }

      if (username.Length > 15) {
         DisplayPopup("Your username is too long");
         return;
      }

      if (string.IsNullOrEmpty(IP)) {
         // This player wants to be a host
         NetworkManager.Singleton.StartHost();
      } else {
         // This player wants to be a client
         IPAddress validated;
         bool valid = IPAddress.TryParse(IP, out validated);

         if (valid && validated.ToString() != "0.0.0.0") {
            NetworkManager.Singleton.JoinGame(validated.ToString());
         } else {
            DisplayPopup("You have not entered a valid IP address");
            return;
         }
      }

      // Make sure user doesn't click anything else while connecting
      canvas.GetComponent<CanvasGroup>().interactable = false;
   }

   public void ExitClicked() {
      #if UNITY_EDITOR
         UnityEditor.EditorApplication.isPlaying = false;
      #endif
      Application.Quit();
   }

   public void HelpClicked() {
      SceneManager.LoadScene("HelpMenu");
   }

   public string getUsername() {
      return usernameField.GetComponent<TMP_InputField>().text;
   }
}
