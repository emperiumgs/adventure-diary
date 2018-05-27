using UnityEngine;
using System.Collections;

public enum TileTypes
{
    Question,
    Advance,
    Return,
    Stop,
    Bonus,
    Minigame,
    Surprise
}

public abstract class Tile : MonoBehaviour
{
    protected static MatchManager manager;
    protected static MatchCanvas canvas;
    protected static AudioController audioCtrl;

    protected virtual void Awake()
    {
        if (manager == null)
        {
            manager = MatchManager.instance;
            canvas = MatchCanvas.instance;
            audioCtrl = AudioController.instance;
        }
    }

    public abstract void Action();
}