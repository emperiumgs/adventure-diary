using UnityEngine;
using System;

public static class JsonUtilityExtended
{
    public static T[] FromJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {   
        // Ignore Warning displayed for unassigned variable
        #pragma warning disable 0649
        public T[] array;
        #pragma warning restore 0649
    }
}