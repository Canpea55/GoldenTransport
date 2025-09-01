[System.Serializable]
public class Sticker
{
    public int id;
    public string custid;
    public int list_number;
    public string cust_name;
    public string storage_location;
    public string descriptions;
}

// Wrapper class for Unity JsonUtility
[System.Serializable]
public class StickerList
{
    public Sticker[] stickers;
}
