using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MatchManager : Singleton<MatchManager>
{
    public AnimationCurve movementCurve;
    [HideInInspector]
    public Dictionary<int, Tile[]> levelTiles;
    public Transform playersSocket;
    [HideInInspector]
    public Player[] players;
    public int round { get; private set; }
    
    public Player curPlayer
    {
        get
        {
            return players[curIndex];
        }
    }

    const float CAMERA_FOCUS_SPEED = 1.5f;
    const float MOVEMENT_TIME = .6f;

    delegate void NextAction();

    MinigameController minigame;
    BookController book;
    MatchCanvas canvas;
    GameCamera matchCamera;
    bool[] playedMinigames;
    bool pendingMinigame;
    int minigameIndex,
        curIndex,
        curPage;

    void Awake()
    {
        matchCamera = GameCamera.instance;
        canvas = MatchCanvas.instance;
        book = BookController.instance;
        minigame = MinigameController.instance;
    }

    #region PlayerSetup
    public void SelectPlayers(int count)
    {
        players = new Player[count];
    }

    public void ConfirmPlayer(string name, int index, Animator obj)
    {
        players[index] = new Player(name, obj);
    }
    #endregion

    #region PlayerStatsControl
    public void AddScore(int score)
    {
        if (curPlayer.doubled)
        {
            players[curIndex].doubled = false;
            players[curIndex].score += score * 2;
        }
        else
            players[curIndex].score += score;
    }

    public void RightAnswer()
    {
        players[curIndex].rightAnswers++;
    }

    public void MinigameWon()
    {
        players[curIndex].minigamesWon++;
    }

    public void BlockPlayer()
    {
        players[curIndex].blocked = true;
    }

    public void DoublePlayer()
    {
        players[curIndex].doubled = true;
    }

    /// <summary>
    /// Enables all players' game objects on the given page number
    /// </summary>
    void DisplayPagePlayers(int pageNum)
    {
        foreach (Player p in players)
        {
            if (p.curPage == pageNum)
                p.obj.gameObject.SetActive(true);
        }
    }

    void HideAllPlayers()
    {
        foreach (Player p in players)
            p.obj.gameObject.SetActive(false);
    }
    #endregion

    #region TurnOrderConfiguration
    public void ConfigureTurnOrder()
    {
        if (players.Length > 1)
            canvas.StartTurnOrder();
        else
            StartMatch();
    }

    public void SetTurnPower(int turnPower)
    {
        players[curIndex].turnPower = turnPower;
        canvas.UpdateTurnOrder();
        curIndex++;
        if (curIndex == players.Length)
        {
            curIndex = 0;
            canvas.EndTurnOrder();
            StartMatch();
        }
        else
            canvas.RollTurnOrder();
    }
    #endregion

    public void StartMatch()
    {
        book.RemoveInactivePages();
        // Setup Player Order
        players = players.OrderByDescending(p => p.turnPower).ToArray();
        // Arrange players in their positions
        levelTiles[0][0].Action();
        // Setup round and turn logic
        round = 1;
        // Setup Match Minigame
        if (players.Length > 1)
        {
            minigame.matchMinigame.ConfigureMinigame(players.Length);
            playedMinigames = new bool[levelTiles.Count - 1];
        }
        // Load Interface
        canvas.StartMatch();
        canvas.TogglePlayerName();
    }

    public void StartTurn(int movement)
    {
        players[curIndex].diceRolls += movement;
        StartCoroutine(StartTurnRoutine(movement));
    }

    public void EndTurn()
    {
        StartCoroutine(EndTurnRoutine());
    }

    public void TurnPass()
    {
        // There is a next player and he is blocked, skip through him
        if (curIndex + 1 < players.Length && players[curIndex + 1].blocked)
        {
            players[curIndex + 1].blocked = false;
            curIndex++;
        }
        if (curIndex < players.Length - 1)
            curIndex++;
        else
        {
            if (players[0].blocked)
            {
                players[0].blocked = false;
                curIndex = players.Length > 1 ? 1 : 0;
            }
            else
                curIndex = 0;
            round++;
        }
    }

    public IEnumerator StartTurnRoutine(int totalMove)
    {
        yield return CheckPageDifference();
        yield return matchCamera.RedirectTo(curPlayer.obj.transform, CAMERA_FOCUS_SPEED);
        // Check if close to the end to subtract movement
        int lastPage = levelTiles.Count - 1,
            lastPageTiles = levelTiles[lastPage].Length - 1,
            curTile = curPlayer.curPageTile;
        if (curPlayer.curPage == lastPage)
        {
            if (curTile + totalMove > lastPageTiles)
                totalMove = lastPageTiles - curTile;
        }
        StartCoroutine(PlayerMovement(totalMove, true, true));
    }

    public IEnumerator EndTurnRoutine()
    {
        yield return matchCamera.ToLevelView(CAMERA_FOCUS_SPEED);
        TurnPass();
        yield return CheckPageDifference();
        if (playedMinigames != null && pendingMinigame)
        {
            pendingMinigame = false;
            playedMinigames[minigameIndex] = true;
            minigameIndex++;
            minigame.StartMatchMinigame();
        }
        else
            canvas.NextTurn();
    }

    Coroutine CheckPageDifference()
    {
        if (curPage != curPlayer.curPage)
            return StartCoroutine(PageMovement());
        else
            return null;
    }

    public IEnumerator PageMovement()
    {
        int pageDiff = curPlayer.curPage - curPage;
        while (pageDiff != 0)
        {
            if (pageDiff > 0)
            {
                HideAllPlayers();
                yield return book.OpenNextPage();
                pageDiff--;
                curPage++;
            }
            else
            {
                HideAllPlayers();
                yield return book.OpenPreviousPage();
                pageDiff++;
                curPage--;
            }
        }
        DisplayPagePlayers(curPage);
    }

    public IEnumerator PlayerMovement(int amount, bool forward, bool activateTile)
    {
        Transform t = curPlayer.obj.transform;
        Vector3 startPos,
            startPoint,
            nextPoint,
            nextPos,
            diff;
        Quaternion targetRotation;
        float time = 0;
        int passes = 0;
        while (passes < amount)
        {
            startPos = t.position;
            startPoint = levelTiles[curPlayer.curPage][curPlayer.curPageTile].transform.position;
            // Check transition between pages
            #region Page Transition
            if (forward && curPlayer.curPageTile + 1 >= levelTiles[curPlayer.curPage].Length)
            {
                yield return matchCamera.ToLevelView(CAMERA_FOCUS_SPEED);
                HideAllPlayers();
                yield return book.OpenNextPage();
                curPage++;
                players[curIndex].curPage++;
                players[curIndex].curPageTile = 0;
                DisplayPagePlayers(curPage);
                t.position = levelTiles[curPlayer.curPage][curPlayer.curPageTile].transform.position - (startPoint - startPos);
                yield return matchCamera.RedirectTo(curPlayer.obj.transform, CAMERA_FOCUS_SPEED);
                if (players.Length > 1 && curPage > minigameIndex && !playedMinigames[minigameIndex])
                    pendingMinigame = true;
            }
            else if (!forward && curPlayer.curPageTile == 0)
            {
                yield return matchCamera.ToLevelView(CAMERA_FOCUS_SPEED);
                HideAllPlayers();
                yield return book.OpenPreviousPage();
                curPage--;
                players[curIndex].curPage--;
                players[curIndex].curPageTile = levelTiles[curPlayer.curPage].Length - 1;
                DisplayPagePlayers(curPage);
                t.position = levelTiles[curPlayer.curPage][curPlayer.curPageTile].transform.position - (startPoint - startPos);
                yield return matchCamera.RedirectTo(curPlayer.obj.transform, CAMERA_FOCUS_SPEED);
            }
            #endregion
            else
            {
                curPlayer.obj.SetBool("walk", true);
                nextPoint = levelTiles[curPlayer.curPage][curPlayer.curPageTile + (forward ? 1 : -1)].transform.position;
                nextPos = nextPoint - (startPoint - startPos);
                diff = nextPos - startPos;
                targetRotation = Quaternion.LookRotation(diff);
                time = 0;
                while (time < MOVEMENT_TIME)
                {
                    time += Time.deltaTime;
                    t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * 5);
                    t.position = startPos + diff * movementCurve.Evaluate(time / MOVEMENT_TIME);
                    yield return null;
                }
                t.position = nextPos;
                players[curIndex].curPageTile += forward ? 1 : -1;
                curPlayer.obj.SetBool("walk", false);
            }
            passes++;
        }
        // Executes current tile action
        if (activateTile)
            levelTiles[curPlayer.curPage][curPlayer.curPageTile].Action();
    }
}