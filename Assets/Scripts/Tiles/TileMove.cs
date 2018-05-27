using UnityEngine;
using System.Collections;

public class TileMove : TileAction
{
    static UIMove ui;

    [HideInInspector]
    public bool forward;

    protected override void Awake()
    {
        base.Awake();
        if (ui == null)
            ui = UIMove.instance;
        score = forward ? 15 : 10;
    }

    public override void Action()
    {
        audioCtrl.source.PlayOneShot(audioCtrl.tileSounds[forward ? (int)TileTypes.Advance : (int)TileTypes.Return]);
        StartCoroutine(ui.DisplayAction(this));
    }
}