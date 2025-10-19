using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small helper to allow JsonUtility to parse a top-level JSON array.
/// Usage: JsonUtilityWrapper.FromJsonList<YourType>(jsonArrayString)
/// </summary>
public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> list;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        string newJson = "{\"list\":" + json + "}";
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.list ?? new List<T>();
    }
}