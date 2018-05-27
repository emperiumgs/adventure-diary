using UnityEngine;
using System.Collections;

public class TileModifier : TileAction
{
    static UIModifier ui;

    // Acts as a Bonus if positive, or as a Stop if negative
    [HideInInspector]
    public bool positive;

    protected override void Awake()
    {
        base.Awake();
        if (ui == null)
            ui = UIModifier.instance;
        score = positive ? 30 : 0;
    }

    public override void Action()
    {
        audioCtrl.source.PlayOneShot(audioCtrl.tileSounds[positive ? (int)TileTypes.Bonus : (int)TileTypes.Stop]);
        StartCoroutine(ui.DisplayAction(this));
    }
}