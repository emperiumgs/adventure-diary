using UnityEngine;
using System.Collections;

public class TileQuestion : TileAction
{
    [HideInInspector]
    public Question question;
    [HideInInspector]
    public int page;

    protected override void Awake()
    {
        base.Awake();
        score = 20;
    }

    public void RightAnswer()
    {
        manager.RightAnswer();
    }

    public override void Action()
    {
        audioCtrl.source.PlayOneShot(audioCtrl.tileSounds[(int)TileTypes.Question]);
        question = MatchQuestions.instance.GetQuestion(page);
        canvas.StartCoroutine(canvas.LoadQuestion(question));
    }
}