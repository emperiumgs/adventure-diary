using UnityEngine;
using System.Collections;

public class TileSurprise : TileAction
{
    [HideInInspector]
    public Material[] mats;

    TileModifier mod;
    TileQuestion quest;
    TileMinigame game;
    TileMove move;

    Renderer[] rends;
    Material mat;

    Material myMat
    {
        set
        {
            foreach (Renderer r in rends)
                r.material = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        rends = GetComponentsInChildren<Renderer>(true);
        mod = GetComponent<TileModifier>();
        quest = GetComponent<TileQuestion>();
        game = GetComponent<TileMinigame>();
        move = GetComponent<TileMove>();
        mod.score = quest.score = game.score = move.score = 15;
        mat = rends[0].material;
    }

    public override void Action()
    {
        audioCtrl.source.PlayOneShot(audioCtrl.tileSounds[(int)TileTypes.Surprise]);
        StartCoroutine(RandomizeTile());
    }

    IEnumerator RandomizeTile()
    {
        float time = 0;
        int length = mats.Length,
            index = 0;
        while (time < 2)
        {
            time += .2f;
            index = Random.Range(0, length);
            myMat = mats[index];
            yield return new WaitForSeconds(.2f);
        }
        switch (index)
        {
            case (int)TileTypes.Question:
                quest.Action();
                break;
            case (int)TileTypes.Advance:
                move.forward = true;
                move.Action();
                break;
            case (int)TileTypes.Return:
                move.forward = false;
                move.Action();
                break;
            case (int)TileTypes.Stop:
                mod.positive = false;
                mod.Action();
                break;
            case (int)TileTypes.Bonus:
                mod.positive = true;
                mod.Action();
                break;
            case (int)TileTypes.Minigame:
                game.Action();
                break;
        }
        myMat = mat;
    }
}