using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum RequestMethod
{
    GET,
    POST,
    DELETE,
    PUT,
    PATCH, // For partial updates
    HEAD,  // To retrieve headers only
    OPTIONS // To determine allowed methods
}

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy duplicate
            return;
        }

        Instance = this;
    }

    void Start()
    {
        //foreach (RequestMethod method in Enum.GetValues(typeof(RequestMethod)))
        //{
        //    Debug.Log(method.ToString());
        //}
    }

    public void Handshake(Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(Request(
            "/handshake",
            RequestMethod.GET,
            onSuccess,  // Pass the success callback down
            onError     // Pass the error callback down
        ));
    }

    public IEnumerator Request(
    string uri,
    RequestMethod method)
    {
        // Call the overload that has callbacks, passing null to them
        yield return Request(uri, method, null, null);
    }

    public IEnumerator Request(
        string uri,
        RequestMethod method,
        Action<string> onSuccess,
        Action<string> onError)
    {
        // This just calls your original function, passing 'null' for the data.
        // We use <object> as a placeholder type since it's not being used.
        yield return Request<object>(uri, method, onSuccess, onError, null);
    }

    public IEnumerator Request<T>(
        string uri,
        RequestMethod method,
        Action<string> onSuccess,     // <-- CALLBACK FOR SUCCESS
        Action<string> onError,       // <-- CALLBACK FOR ERROR
        T? serializableData = default
    ) where T : class
    {
        string url = "http://" + SettingsManager.Instance.GetServerIP() + "/api" + uri;
        Debug.Log( url );
        using (UnityWebRequest req = new UnityWebRequest(url, method.ToString()))
        {
            if (serializableData != null)
            {
                string json = JsonUtility.ToJson(serializableData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.SetRequestHeader("Content-Type", "application/json");
            }

            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            string DbMsg = method.ToString() + " : ";
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = DbMsg + req.error + " - " + req.downloadHandler.text;
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg); // <-- CALL ERROR CALLBACK
                yield break;
            }
            else
            {
                string responseText = req.downloadHandler.text;
                Debug.Log(DbMsg + responseText);
                onSuccess?.Invoke(responseText); // <-- CALL SUCCESS CALLBACK WITH DATA
            }
        }
    }
}
