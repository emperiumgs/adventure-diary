using UnityEngine;
using System.Collections;

public class TileAction : Tile
{
    [HideInInspector]
    public int score;

    public virtual void IncrementScore()
    {
        manager.AddScore(score);
    }

    public override void Action()
    {
    }
}