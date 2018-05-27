using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIMove : UIAction<UIMove>
{
    public GameObject advanceContent,
        returnContent;
    public Button advanceContinue,
        returnContinue;

    const int STEPS = 2;
    
    MatchManager manager;
    TileMove tile;

    protected override void Awake()
    {
        base.Awake();
        manager = MatchManager.instance;
        advanceContinue.onClick.AddListener(EndAction);
        returnContinue.onClick.AddListener(EndAction);
    }

    protected override void EndAction()
    {
        inProgress = false;
        advanceContent.SetActive(false);
        returnContent.SetActive(false);
    }

    public override IEnumerator DisplayAction(TileAction originTile)
    {
        tile = originTile as TileMove;
        inProgress = true;
        if (tile.forward)
            advanceContent.SetActive(true);
        else
            returnContent.SetActive(true);
        while (inProgress)
            yield return null;
        tile.IncrementScore();
        yield return new WaitForSeconds(1);
        yield return StartCoroutine(manager.PlayerMovement(STEPS, tile.forward, false));
        manager.EndTurn();
    }
}