using UnityEngine;
using System.Collections;

public abstract class UIAction<T> : MonoBehaviour where T : UIAction<T>
{
    public static T instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<T>() as UIAction<T>;
            return _instance as T;
        }
    }
    protected static UIAction<T> _instance;

    protected bool inProgress;

    protected virtual void Awake()
    {
        _instance = this;
    }

    protected abstract void EndAction();

    public T GetInstance()
    {
        return instance;
    }

    public abstract IEnumerator DisplayAction(TileAction originTile);
}