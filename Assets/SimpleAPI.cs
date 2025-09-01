using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleAPI : MonoBehaviour
{
    IEnumerator Start()
    {
        string serverip = PlayerPrefs.GetString("ServerIP");
        string url = "http://" + serverip + "/api/hello";

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
            //yield return StartCoroutine(GetStickers(serverip));
        }
        else
        {
            Debug.LogError("Error: " + req.error);
        }
    }

    IEnumerator GetStickers(string ip)
    {
        string url = "https://" + ip + "/api/stickers";

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"stickers\":" + req.downloadHandler.text + "}";
            StickerList stickerList = JsonUtility.FromJson<StickerList>(json);
            foreach (Sticker s in stickerList.stickers)
            {
                Debug.Log($"ID: {s.id}, Name: {s.cust_name}, Location: {s.storage_location}");
            }
        }
        else
        {
            Debug.LogError(req.error);
        }
    }

    IEnumerator AddSticker(string ip)
    {
        string url = "https://" + ip + "/api/stickers";
        string json = "{\"custid\":\"CUST004\",\"list_number\":4,\"cust_name\":\"Anna\",\"storage_location\":\"Warehouse D\",\"descriptions\":\"Handle gently\"}";

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log("Sticker added: " + req.downloadHandler.text);
        else
            Debug.LogError(req.error);
    }

}
