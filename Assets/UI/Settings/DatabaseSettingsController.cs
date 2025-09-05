using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TextField = UnityEngine.UIElements.TextField;

public class DatabaseSettingsController : MonoBehaviour
{
    public VisualElement ui;

    public TextField serverIP;
    public TextField username;
    public TextField password;

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        serverIP = ui.Query<TextField>("ServerIPField");
        if (PlayerPrefs.HasKey("ServerIP"))
        {
            serverIP.value = PlayerPrefs.GetString("ServerIP");
        }
        else
        {
            SetServerIP("192.168.0.28");
        }

        username = ui.Query<TextField>("UsernameField");
        if (PlayerPrefs.HasKey("Username"))
        {
            username.value = PlayerPrefs.GetString("Username");
        }
        else
        {
            SetUsername("root");
        }

        password = ui.Query<TextField>("PasswordField");
        if (PlayerPrefs.HasKey("Password"))
        {
            password.value = PlayerPrefs.GetString("Password");
        }
        else
        {
            SetPassword("");
        }

        serverIP.RegisterValueChangedCallback(evt => { SetServerIP(serverIP.value); });
        username.RegisterValueChangedCallback(evt => { SetUsername(username.value); });
        password.RegisterValueChangedCallback(evt => { SetPassword(password.value); });

    }

    private void SetServerIP(string serverIP)
    {
        this.serverIP.value = serverIP;
        PlayerPrefs.SetString("ServerIP", this.serverIP.value);
    }

    private void SetUsername(string username)
    {
        this.username.value = username;
        PlayerPrefs.SetString("Username", this.username.value);
    }

    private void SetPassword(string password)
    {
        this.password.value = password;
        PlayerPrefs.SetString("Password", this.password.value);
    }
}
