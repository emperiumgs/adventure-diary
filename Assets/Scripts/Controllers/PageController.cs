using UnityEngine;

public abstract class PageController : MonoBehaviour
{
    protected MatchManager manager;
    [HideInInspector]
    public Popup nextPopup;

    protected virtual void Awake()
    {
        manager = MatchManager.instance;
    }

    public abstract void LoadPage();
}