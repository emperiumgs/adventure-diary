using UnityEngine;

public struct Player
{
    public Animator obj;
    public string name;
    public int[] matchMinigameResults;
    public int curPage,
        curPageTile,
        turnPower,
        rightAnswers,
        diceRolls,
        minigamesWon,
        score;
    public bool blocked,
        doubled;

    public Player(string name, Animator obj)
    {
        this.name = name;
        this.obj = obj;
        matchMinigameResults = new int[2];
        turnPower = curPage = curPageTile = rightAnswers = diceRolls = minigamesWon = score = 0;
        blocked = doubled = false;
    }
}