[System.Serializable]
public class Sticker
{
    public int id;
    public string custid;
    public int folder_id;
    public int list_number;
    public string cust_name;
    public string descriptions;
    public string created_at;
    public string updated_at;
}

// Wrapper class for Unity JsonUtility
[System.Serializable]
public class StickerList
{
    public Sticker[] stickers;
}
