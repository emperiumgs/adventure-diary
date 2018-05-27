using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BrazilMinigame : Singleton<BrazilMinigame>, IMinigameLoader
{
    public SoccerBall[] balls;
    public GameObject[] interfaces,
        rotationCtrls;
    public Transform center;
    public Transform[] pivots,
        arrows;
    public Image[] bars;
    public AudioClip ballShoot,
        ballRoll,
        ballCollide,
        ballEnd,
        victory;
    public AnimationCurve sliderMovement;

    const int MAX_ROTATE = 30,
        MIN_ROTATE = -30,
        ROTATE_AMOUNT = 5,
        MAX_TURNS = 2;

    public static int minigameTurn { get; private set; }

    AudioController audioCtrl;
    MatchManager manager;
    MatchCanvas canvas;
    Player[] players;
    bool ballInput;
    int curIndex,
        curTurn,
        curRotate;

    void Awake()
    {
        manager = MatchManager.instance;
        canvas = MatchCanvas.instance;
        audioCtrl = AudioController.instance;
        foreach (Transform t in pivots)
            t.LookAt(center);
    }

    void ResetElements()
    {
        curIndex = curTurn = curRotate = 0;
        // Make each pivot face the center, then make each ball have its rotation
        int max = pivots.Length;
        for (int i = 0; i < max; i++)
        {
            balls[i].gameObject.SetActive(true);
            balls[i].DisableInteraction();
            // Second ball will only be active on the second turn
            balls[i + max].gameObject.SetActive(false);
            balls[i + max].DisableInteraction();
            // Both of them have the same properties
            balls[i].isKinematic = balls[i + max].isKinematic = true;
            balls[i].position = balls[i + max].position = pivots[i].position + Vector3.up * .35f;
            balls[i].rotation = balls[i + max].rotation = pivots[i].rotation;
            arrows[i].localEulerAngles = new Vector3(0, 0, -pivots[i].localEulerAngles.y);
            interfaces[i].SetActive(false);
        }
    }

    void EndMinigame()
    {
        audioCtrl.source.PlayOneShot(victory);
        Collider[] hits;
        Collider golden = null;
        int i;
        // Sphere Raycast to center
        hits = Physics.OverlapSphere(center.position, .25f, 1 << gameObject.layer);
        if (hits.Length > 0)
        {
            // Find the Golden Score
            i = 0;
            bool found = false;
            while (i < hits.Length && !found)
            {
                if (hits[i].GetComponent<SoccerBall>() != null)
                {
                    golden = hits[i];
                    found = true;
                }
                i++;
            }
        }
        // Sphere Raycast to extents
        List<SoccerBall> hitBalls = null;
        hits = Physics.OverlapSphere(center.position, 1.75f, 1 << gameObject.layer);
        if (hits.Length > 0)
        {
            hitBalls = new List<SoccerBall>(hits.Length);
            i = 0;
            while (i < hits.Length)
            {
                if (hits[i].GetComponent<SoccerBall>() != null && hits[i] != golden)
                    hitBalls.Add(hits[i].GetComponent<SoccerBall>());
                i++;
            }
        }
        // Assign points to players
        int[] results = new int[players.Length];
        // Golden Ball Assignment
        if (golden != null)
            results[golden.GetComponent<SoccerBall>().id] = 3;
        if (hitBalls != null)
            for (i = 0; i < hitBalls.Count; i++)
                results[hitBalls[i].id]++;
        // Define Score
        for (i = 0; i < results.Length; i++)
            players[i].matchMinigameResults[minigameTurn] = results[i];
        players = players.OrderByDescending(p => p.matchMinigameResults[minigameTurn]).ToArray();
        // Define standing
        bool relay;
        int prevScore = 0,
            prevStanding = -1;
        for (i = 0; i < players.Length; i++)
        {
            relay = true;
            // Check for draws
            if (i > 0)
            {
                if (players[i].matchMinigameResults[minigameTurn] == prevScore)
                {
                    players[i].matchMinigameResults[minigameTurn] = prevStanding;
                    relay = false;
                }
            }
            if (relay)
            {
                prevScore = players[i].matchMinigameResults[minigameTurn];
                // Assign standing
                players[i].matchMinigameResults[minigameTurn] = prevStanding + 1;
                prevStanding = players[i].matchMinigameResults[minigameTurn];
            }
        }
        // Assign values to original players
        int j = 0;
        for (i = 0; i < players.Length; i++)
        {
            relay = false;
            j = 0;
            while (!relay && j < players.Length)
            {
                if (manager.players[i].name == players[j].name)
                {
                    manager.players[i].matchMinigameResults[minigameTurn] = players[j].matchMinigameResults[minigameTurn];
                    relay = true;
                }
                j++;
            }
        }
        canvas.ToggleButtons();
        canvas.TogglePlayerName();
        canvas.DisplayMinigameResults(players);
        minigameTurn++;
    }

    public void ConfigureMinigame(int playerCount)
    {
        minigameTurn = 0;
        if (manager == null)
            manager = MatchManager.instance;
        int i,
            max = pivots.Length;
        // Cycle through each arrays and remove unecessary stuff
        for (i = max - 1; i > playerCount - 1; i--)
        {
            Destroy(balls[i].gameObject);
            Destroy(balls[i + max].gameObject);
            Destroy(pivots[i].gameObject);
            Destroy(interfaces[i].gameObject);
        }
        max = playerCount;
        SoccerBall[] newBalls = new SoccerBall[max * 2];
        Transform[] newPivots = new Transform[max],
            newArrows = new Transform[max];
        GameObject[] newInterfaces = new GameObject[max],
            newRotationCtrls = new GameObject[max];
        Image[] newBars = new Image[max];
        for (i = 0; i < max; i++)
        {
            newBalls[i] = balls[i];
            newBalls[i + max] = balls[i + pivots.Length];
            newPivots[i] = pivots[i];
            newArrows[i] = arrows[i];
            newInterfaces[i] = interfaces[i];
            newRotationCtrls[i] = rotationCtrls[i];
            newBars[i] = bars[i];
        }
        balls = newBalls;
        pivots = newPivots;
        arrows = newArrows;
        interfaces = newInterfaces;
        rotationCtrls = newRotationCtrls;
        bars = newBars;
    }

    public void LoadMinigame()
    {
        canvas.ToggleButtons();
        players = manager.players.OrderByDescending(p => p.curPage).ThenByDescending(p => p.curPageTile).ToArray();
        canvas.UpdatePlayerName(players[curIndex].name);
        ResetElements();
        balls[curIndex].EnableInteraction();
        interfaces[curIndex].SetActive(true);
        rotationCtrls[curIndex].SetActive(true);
        StartCoroutine(BallForce());
    }

    public void ClockwiseRotate()
    {
        if (curRotate < MAX_ROTATE)
        {
            curRotate += ROTATE_AMOUNT;
            Transform t = balls[curIndex + curTurn * balls.Length / 2].transform;
            t.Rotate(Vector3.up * ROTATE_AMOUNT);
            arrows[curIndex].localEulerAngles = new Vector3(0, 0, -t.localEulerAngles.y);
        }
    }

    public void CounterClockwiseRotate()
    {
        if (curRotate > MIN_ROTATE)
        {
            curRotate -= ROTATE_AMOUNT;
            Transform t = balls[curIndex + curTurn * balls.Length / 2].transform;
            t.Rotate(Vector3.down * ROTATE_AMOUNT);
            arrows[curIndex].localEulerAngles = new Vector3(0, 0, -t.localEulerAngles.y);
        }
    }

    public void BallInput()
    {
        ballInput = true;
    }

    public void NextPlayer()
    {
        curRotate = 0;
        interfaces[curIndex].SetActive(false);
        curIndex++;
        if (curIndex >= pivots.Length)
        {
            curIndex = 0;
            curTurn++;
        }
        int i = curIndex + curTurn * pivots.Length;
        if (curTurn >= MAX_TURNS)
        {
            EndMinigame();
            return;
        }
        else if (curTurn > 0)
            balls[i].gameObject.SetActive(true);
        balls[i].EnableInteraction();
        interfaces[curIndex].SetActive(true);
        rotationCtrls[curIndex].SetActive(true);
        arrows[curIndex].localEulerAngles = new Vector3(0, 0, -pivots[curIndex].localEulerAngles.y);
        canvas.UpdatePlayerName(players[curIndex].name);
        StartCoroutine(BallForce());
    }

    IEnumerator BallForce()
    {
        yield return new WaitUntil(() => ballInput);
        rotationCtrls[curIndex].SetActive(false);
        ballInput = false;
        bars[curIndex].gameObject.SetActive(true);
        float time = 0;
        while (!ballInput)
        {
            time = 0;
            while (!ballInput && time < 1)
            {
                time += Time.deltaTime;
                bars[curIndex].fillAmount = sliderMovement.Evaluate(time);
                yield return null;
            }
        }
        ballInput = false;
        bars[curIndex].gameObject.SetActive(false);
        interfaces[curIndex].SetActive(false);
        int i = curIndex + curTurn * pivots.Length;
        balls[i].DisableInteraction();
        balls[i].Shoot(sliderMovement.Evaluate(time));
    }
}