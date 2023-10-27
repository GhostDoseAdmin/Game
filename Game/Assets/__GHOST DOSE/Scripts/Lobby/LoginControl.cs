using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameManager;
using NetworkSystem;
using Newtonsoft.Json;

public class LoginControl : MonoBehaviour
{
    public GameObject usernameField;
    public GameObject pinField;
    public GameObject yesButton;
    public GameObject noButton;
    public GameObject signOut;
    public GameObject mainMenu;
    public GameObject screenMask;
    private string currentUser;
    private bool setupPin;
    private bool confirmPin;
    private string testNewPin;
    private bool saving;
    private bool checkingUser = false;
    private bool userfound = false;
    private bool authenticating = false;
    private bool newAccount = false;


    public void Start()
    {
        if (PlayerPrefs.GetInt("login_saved") == 1)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "START";
            signOut.SetActive(true);
            usernameField.SetActive(false);

        }
        else
        {
            signOut.SetActive(false);
            usernameField.SetActive(true);
        }
    }
    public void Clicked()
    {
        if (NetworkDriver.instance.connected)
        {
            if (PlayerPrefs.GetInt("login_saved") == 0)
            {
                //AUTHENTICATE PIN
                if (userfound && !authenticating)
                {
                    authenticating = true;
                    NetworkDriver.instance.sioCom.Instance.Emit("login", JsonConvert.SerializeObject(new { username = usernameField.GetComponent<TMP_InputField>().text, pin = pinField.GetComponent<TMP_InputField>().text }), false);
                }

                if (!userfound)
                {
                    if (!saving)
                    {
                        //CHECK USERNAME
                        if (!setupPin && !checkingUser) { checkingUser = true; NetworkDriver.instance.sioCom.Instance.Emit("check_username", JsonConvert.SerializeObject(new { username = usernameField.GetComponent<TMP_InputField>().text }), false); }

                        if (setupPin)
                        {
                            //CHECK PIN LENGTH
                            if (!confirmPin)
                            {
                                if (pinField.GetComponent<TMP_InputField>().text.Length < 4) { GameDriver.instance.WriteGuiMsg("Pin must be 4 numbers", 10f, false, Color.yellow); }
                                //CONFIRM PIN
                                else { GameDriver.instance.WriteGuiMsg("Please confirm your pin", 10f, true, Color.white); confirmPin = true; testNewPin = pinField.GetComponent<TMP_InputField>().text; pinField.GetComponent<TMP_InputField>().text = ""; return; }
                            }
                            //COMPARE PINS
                            if (confirmPin)
                            {
                                //SAVING ACCOUNT
                                if (pinField.GetComponent<TMP_InputField>().text == testNewPin)
                                {
                                    GameDriver.instance.WriteGuiMsg("Saving Account!", 5f, true, Color.white);
                                    NetworkDriver.instance.sioCom.Instance.Emit("save_user", JsonConvert.SerializeObject(new { username = usernameField.GetComponent<TMP_InputField>().text, pin = pinField.GetComponent<TMP_InputField>().text }), false);
                                    saving = true;
                                }
                                //PINS DONT MATCH
                                else { GameDriver.instance.WriteGuiMsg("Pin's don't match! Starting over. Create a PIN", 999f, true, Color.white); confirmPin = false; pinField.GetComponent<TMP_InputField>().text = ""; }
                            }
                        }
                    }
                }
            }
            else { MainMenu(); }
        }
    }
    public void MainMenu()
    {
        if (PlayerPrefs.GetString("username").Length > 0)
        {
            NetworkDriver.instance.USERNAME = PlayerPrefs.GetString("username");
        }
        else { NetworkDriver.instance.USERNAME = usernameField.GetComponent<TMP_InputField>().text; }

        if (newAccount) { GameDriver.instance.WriteGuiMsg("Account Created Successfully!", 5f, false, Color.yellow); }
        yesButton.SetActive(false);
        noButton.SetActive(false);
        mainMenu.SetActive(true);
        signOut.SetActive(false);
        screenMask.SetActive(false);
        this.gameObject.SetActive(false);
 
    }
    public void YesClicked()    {
        PlayerPrefs.SetInt("login_saved", 1);
        PlayerPrefs.SetString("username", usernameField.GetComponent<TMP_InputField>().text);
        GameDriver.instance.WriteGuiMsg("Login saved", 5f, false, Color.yellow);
        MainMenu();
    }
    public void NoClicked()    {
        PlayerPrefs.SetInt("login_saved", 0);
        PlayerPrefs.SetString("username", "");
        GameDriver.instance.WriteGuiMsg("", 0.1f, false, Color.yellow);
        MainMenu();
    }
    
    public void RemoveLogin()
    {
        usernameField.SetActive(true);
        signOut.SetActive(false);
        //PlayerPrefs.SetInt("login_saved", 0);
        PlayerPrefs.DeleteAll();
        GetComponentInChildren<TextMeshProUGUI>().text = "LOGIN";
    }
    
    public void SaveLogin()
    {
        pinField.SetActive(false);
        usernameField.SetActive(false);
        yesButton.SetActive(true);
        noButton.SetActive(true);
        this.gameObject.GetComponent<Image>().enabled = false;
        this.gameObject.GetComponent<Button>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        GameDriver.instance.WriteGuiMsg("Stay logged in?", 999f, false, Color.yellow);

    }
    public void UserFound()
    {
        checkingUser = false;
        userfound = true;
        GameDriver.instance.WriteGuiMsg("Enter PIN to continue", 999f, false, Color.yellow);
        currentUser = usernameField.GetComponent<TMP_InputField>().text;
        pinField.SetActive(true); 
    }
    public void NoUserFound()
    {
        checkingUser = false;
        GameDriver.instance.WriteGuiMsg("Username not found! Create a pin to save account", 999f, false, Color.yellow);
        pinField.SetActive(true); setupPin = true; currentUser = usernameField.GetComponent<TMP_InputField>().text; 
    }
    public void SavingFailed()
    {
        GameDriver.instance.WriteGuiMsg("Save failed!", 999f, false, Color.red);
        setupPin = false; confirmPin = false; currentUser = ""; saving = false; pinField.GetComponent<TMP_InputField>().text = ""; usernameField.GetComponent<TMP_InputField>().text = ""; pinField.SetActive(false);
    }
    public void SavingSuccess()
    {
        newAccount = true;
        //GameDriver.instance.WriteGuiMsg("User saved successfully!", 5f, false, Color.green);
        SaveLogin();
    }
    public void LoginSuccess()
    {
        GameDriver.instance.WriteGuiMsg("Logging in!", 5f, true, Color.green);
        SaveLogin();
    }
    public void LoginFail()
    {
        GameDriver.instance.WriteGuiMsg("Wrong PIN. Login failed!", 999f, false, Color.red);
        authenticating = false;
    }
    private void Update()
    {
        //CANCEL PIN SETUP - username changed
        if (setupPin && currentUser != usernameField.GetComponent<TMP_InputField>().text) { currentUser = usernameField.GetComponent<TMP_InputField>().text; saving = false; userfound = false; checkingUser = false; confirmPin = false; setupPin = false; pinField.GetComponent<TMP_InputField>().text = "";  pinField.SetActive(false); GameDriver.instance.WriteGuiMsg("", 0.1f, false, Color.white); }
        //CANCEL LOGIN
        if (userfound) { if (currentUser != usernameField.GetComponent<TMP_InputField>().text) { currentUser = usernameField.GetComponent<TMP_InputField>().text; userfound = false; pinField.GetComponent<TMP_InputField>().text = ""; pinField.SetActive(false); GameDriver.instance.WriteGuiMsg("", 0.1f, false, Color.white); } }

        //HIDE START BUTTON
        if (NetworkDriver.instance.connected)
        {
            GetComponent<Image>().enabled = true;
            GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        }
        else {
            GetComponent<Image>().enabled = false;
            GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        }
        
    
    }


}
