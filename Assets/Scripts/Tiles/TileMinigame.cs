using UnityEngine;
using System.Collections;

public class TileMinigame : TileAction
{
    static MinigameController ctrl;

    bool done;

    protected override void Awake()
    {
        base.Awake();
        if (ctrl == null)
            ctrl = MinigameController.instance;
        score = 50;
    }

    public override void Action()
    {
        audioCtrl.source.PlayOneShot(audioCtrl.tileSounds[(int)TileTypes.Minigame]);
        ctrl.StartMinigame(this);
    }
}