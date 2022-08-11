using System.Collections;
using System.Collections.Generic;
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
   private GameObject popup;

   private void Awake() {
      Singleton = this;
      SetupPopup();
   }

   private void SetupPopup() {
      popup = Instantiate(Resources.Load("Prefabs/Popup") as GameObject);
      popup.SetActive(false);
      popup.transform.SetParent(canvas.transform, false);
      
      // Make ok button functional
      Transform okBtn = popup.transform.Find("okBtn");
      okBtn.gameObject.GetComponent<Button>().onClick.AddListener(PopupHandler);
   }

   private void PopupHandler() {
      // This method is called when the popup is closed

      // Make the popup inactive
      popup.SetActive(false);

      // Make all inputs and buttons interactable again
      canvas.GetComponent<CanvasGroup>().interactable = true;
   }

   public void StartClicked() {
      string IP = IPField.GetComponent<TMP_InputField>().text;
      string username = usernameField.GetComponent<TMP_InputField>().text;

      // Make sure user has not left username blank
      if (string.IsNullOrEmpty(username)) {
         // Make all inputs and buttons greyed out
         canvas.GetComponent<CanvasGroup>().interactable = false;

         // Display the popup
         popup.SetActive(true);
         Transform errorText = popup.transform.Find("ErrorText");
         errorText.GetComponent<TextMeshProUGUI>().SetText("You have not entered a username");

         return;
      }

      if (string.IsNullOrEmpty(IP)) {
         // This player wants to be a host
         NetworkManager.Singleton.StartHost();
      } else {
         // This player wants to be a client
         NetworkManager.Singleton.JoinGame(IP);
      }

      // Make sure user doesn't click anything else while connecting
      canvas.GetComponent<CanvasGroup>().interactable = false;
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
