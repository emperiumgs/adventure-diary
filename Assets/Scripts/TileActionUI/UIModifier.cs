using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIModifier : UIAction<UIModifier>
{
    public GameObject posContent,
        negContent;
    public Button posButton,
        negButton;
        
    MatchManager manager;
    bool positive;

    protected override void Awake()
    {
        base.Awake();
        manager = MatchManager.instance;
        posButton.onClick.AddListener(EndAction);
        negButton.onClick.AddListener(EndAction);
    }

    protected override void EndAction()
    {
        inProgress = false;
        if (positive)
            posContent.SetActive(false);
        else
            negContent.SetActive(false);
    }

    public override IEnumerator DisplayAction(TileAction originTile)
    {
        inProgress = true;
        TileModifier tile = originTile as TileModifier;
        tile.IncrementScore();
        if (tile.positive)
        {
            manager.DoublePlayer();
            posContent.SetActive(true);
        }
        else
        {
            manager.BlockPlayer();
            negContent.SetActive(true);
        }
        positive = tile.positive;
        while (inProgress)
            yield return null;
        manager.EndTurn();
    }
}