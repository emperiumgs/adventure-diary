using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MatchCanvas : Singleton<MatchCanvas>
{
    enum QuestionFlags
    {
        None = 0,
        Done = 1,
        AnsweredTrue = 2,
        AnsweredFalse = 4,
        TimedOut = 8,
        Finished = 16
    }

    [Header("Turn Order")]
    public GameObject turnInterface;
    public Text playerName,
        turnOrder;
    [Header("Match Interface")]
    public GameObject matchInterface;
    public GameObject buttons,
        menuButton;
    public RectTransform nameHolder;
    public Text matchName;
    [Header("Match Minigame")]
    public GameObject matchMinigame;
    public Text standings,
        names;
    [Header("Question")]
    public GameObject question;
    public GameObject answers,
        questionContinue;
    public Text questionText;
    public AudioClip right,
        wrong;
    [Header("Menu")]
    public GameObject menu;
    public GameObject options;
    [Header("Scoreboard")]
    public GameObject scoreboard;
    public GameObject scoreboardContinue;
    public Text extras;
    public Image extrasImage;
    public Sprite[] extrasSprites;
    public RectTransform[] rows;
    public Text[] playerNames,
        playerStandings,
        playerExtras,
        playerScores;
    [Header("Dice")]
    public Dice dice;

    const float transTime = .25f;

    QuestionFlags qFlags;

    MinigameController ctrl;
    AudioController audioCtrl;
    MatchManager manager;
    string[] extrasTexts = { "Perguntas", "Dado", "Extras" };

    void Awake()
    {
        manager = MatchManager.instance;
        ctrl = MinigameController.instance;
        audioCtrl = AudioController.instance;
    }

    #region TurnOrderControl
    public void StartTurnOrder()
    {
        turnInterface.SetActive(true);
        RollTurnOrder();
    }

    public void RollTurnOrder()
    {
        playerName.text = manager.curPlayer.name;
        dice.ActivateDice(manager.SetTurnPower);
    }

    public void UpdateTurnOrder()
    {
        IEnumerable<Player> query = manager.players.OrderByDescending(x => x.turnPower);
        string s = "";
        foreach (Player pl in query)
            s += pl.name + ": " + pl.turnPower + "\n";
        turnOrder.text = s;
    }

    public void EndTurnOrder()
    {
        turnInterface.SetActive(false);
    }
    #endregion

    #region InterfaceControls
    public void ToggleElement(GameObject el)
    {
        el.SetActive(!el.activeSelf);
    }

    public void ToggleMenu()
    {
        ToggleElement(menu);
        PauseControl();
    }

    public void ToggleOptions()
    {
        ToggleElement(options);
        PauseControl();
    }

    public void ToggleMenuButton()
    {
        ToggleElement(menuButton);
    }

    public void TogglePlayerName()
    {
        ToggleElement(nameHolder.gameObject);
    }

    public void ToggleButtons()
    {
        ToggleElement(buttons);
    }

    public void DisplayMinigameResults(Player[] players)
    {
        matchMinigame.SetActive(true);
        standings.text = "";
        names.text = "";
        string newLine = "\n";
        int minigameTurn = BrazilMinigame.minigameTurn;
        for (int i = 0; i < players.Length; i++)
        {
            standings.text += players[i].matchMinigameResults[minigameTurn] + 1;
            names.text += players[i].name;
            if (i != players.Length - 1)
            {
                standings.text += newLine;
                names.text += newLine;
            }
        }
    }

    public void UpdatePlayerName(string name)
    {
        matchName.text = name;
    }
    #endregion

    #region QuestionControl
    public IEnumerator LoadQuestion(Question q)
    {
        ctrl.conclusion = ctrl.EndQuestion;
        //ctrl.StartCoroutine(ctrl.StartCountdown());
        ctrl.EnableComponents(false, true);
        qFlags = QuestionFlags.None;
        ToggleElement(question);
        ToggleElement(answers);
        questionText.text = q.question;
        yield return new WaitWhile(() => qFlags == QuestionFlags.None);
        ctrl.StopAllCoroutines();
        ToggleElement(answers);
        ToggleElement(questionContinue);
        if (((qFlags & QuestionFlags.AnsweredTrue) == QuestionFlags.AnsweredTrue && q.valid) ||
            ((qFlags & QuestionFlags.AnsweredFalse) == QuestionFlags.AnsweredFalse && !q.valid))
        {
            manager.RightAnswer();
            audioCtrl.source.PlayOneShot(right);
        }
        else
            audioCtrl.source.PlayOneShot(wrong);
        questionText.text = q.rightAnswer;
        yield return new WaitUntil(() => (qFlags & QuestionFlags.Finished) == QuestionFlags.Finished);
        ToggleElement(questionContinue);
        ToggleElement(question);
        ctrl.DisableComponents(false, true);
        manager.EndTurn();
    }

    public void AnswerQuestion(bool answer)
    {
        qFlags |= answer ? QuestionFlags.AnsweredTrue : QuestionFlags.AnsweredFalse;
    }

    public void TimesUp()
    {
        qFlags |= QuestionFlags.TimedOut;
    }

    public void EndQuestion()
    {
        qFlags |= QuestionFlags.Finished;
    }
    #endregion

    public void StartMatch()
    {
        matchInterface.SetActive(true);
        NextTurn();
    }

    public void NextTurn()
    {
        Player p = manager.curPlayer;
        matchName.text = p.name;
        dice.ActivateDice(manager.StartTurn);
    }

    public void PauseControl()
    {
        Time.timeScale = menu.activeSelf || options.activeSelf ? 0 : 1;
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator LoadScoreboard()
    {
        audioCtrl.TransitionToSoundtrack(SoundtrackType.Victory, .2f);
        scoreboard.SetActive(true);
        ToggleButtons();
        TogglePlayerName();
        // Setup Rows count
        int max = manager.players.Length,
            i;
        if (max != rows.Length)
        {
            for (i = max; i < rows.Length; i++)
                rows[i].gameObject.SetActive(false);
            RectTransform[] newRows = new RectTransform[max];
            for (i = 0; i < newRows.Length; i++)
                if (rows[i].gameObject.activeSelf)
                    newRows[i] = rows[i];
            rows = newRows;
        }
        // Setup players, ordering by higher score
        Player[] players = manager.players.OrderByDescending(x => x.score).ToArray();
        for (i = 0; i < rows.Length; i++)
        {
            playerNames[i].text = players[i].name;
            playerStandings[i].text = (i + 1).ToString();
            playerScores[i].text = players[i].score.ToString();
        }
        yield return new WaitForSeconds(1);
        // Iterate through extras
        int extraScore = 0;
        for (int e = 0; e < extrasTexts.Length; e++)
        {
            if (e > 0)
            {
                extras.text = extrasTexts[e];
                extrasImage.sprite = extrasSprites[e];
            }
            for (i = 0; i < players.Length; i++)
            {
                // Add score and check for new standings
                extraScore = 0;
                switch (e)
                {
                    case 0:
                        extraScore = players[i].rightAnswers * 30;
                        break;
                    case 1:
                        extraScore = players[i].diceRolls * 10;
                        break;
                    case 2:
                        extraScore = players[i].minigamesWon * 25;
                        foreach (int s in players[i].matchMinigameResults)
                            extraScore += (s + 1) * 50;
                        break;
                }
                playerExtras[i].text = extraScore.ToString();
                players[i].score += extraScore;
                playerScores[i].text = players[i].score.ToString();
            }
            players = players.OrderByDescending(x => x.score).ToArray();
            yield return new WaitForSeconds(2);
            foreach (Text t in playerExtras)
                t.text = "";
        }
        scoreboardContinue.SetActive(true);
    }
}