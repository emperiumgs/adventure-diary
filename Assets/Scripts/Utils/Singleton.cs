using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance
    {
        get
        {
            if (_instance == null)
                _instance = (T)FindObjectOfType(typeof(T));
            return _instance;
        }
    }
    static T _instance;
}
