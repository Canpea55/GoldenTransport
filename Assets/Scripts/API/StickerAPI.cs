using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StickerAPI : MonoBehaviour
{
    public IEnumerator GetStickers(string ip, System.Action<StickerList> callback)
    {
        string url = "http://" + ip + "/api/stickers";

        UnityWebRequest req = UnityWebRequest.Get(url);
        Debug.Log("Request: " + url);
        yield return req.SendWebRequest(); //actually wait for request

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request Result: " + req.result.ToString());
            string json = "{\"stickers\":" + req.downloadHandler.text + "}";
            StickerList stickerList = JsonUtility.FromJson<StickerList>(json);
            callback?.Invoke(stickerList);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
            callback?.Invoke(null);
        }
    }
}
