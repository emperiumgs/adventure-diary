using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public interface IMinigameLoader
{
    void LoadMinigame();
}
public class MinigameController : Singleton<MinigameController>
{
    public Text timer;
    public GameObject common,
        special;
    public GameObject[] minigames;

    public BrazilMinigame matchMinigame { get; private set; }

    public delegate void NextAction();

    public NextAction conclusion;

    AudioController audioCtrl;
    TileMinigame curTile;
    MatchManager manager;
    MatchCanvas canvas;
    Coroutine countdown;
    GameCamera cam;
    GameObject curMinigame;
    GameObject content;
    string[] numStrings = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
    "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"};
    int time = 30;

    void Awake()
    {
        content = transform.GetChild(0).gameObject;
        cam = GameCamera.instance;
        audioCtrl = AudioController.instance;
        canvas = MatchCanvas.instance;
        manager = MatchManager.instance;
        matchMinigame = special.GetComponent<BrazilMinigame>();
    }

    void EndMinigame()
    {
        curMinigame.SetActive(false);
        curTile.IncrementScore();
        DisableComponents();
        manager.EndTurn();
    }

    public void EnableComponents()
    {
        EnableComponents(true);
    }
    public void EnableComponents(bool simple)
    {
        EnableComponents(true, simple);
    }
    public void EnableComponents(bool optmizeCamera, bool simple)
    {
        if (optmizeCamera)
            cam.SetCameraActive(false);
        content.SetActive(true);
        if (simple)
        {
            common.SetActive(true);
            countdown = StartCoroutine(StartCountdown());
        }
        else
            special.SetActive(true);
    }

    public void DisableComponents()
    {
        DisableComponents(true);
    }
    public void DisableComponents(bool simple)
    {
        DisableComponents(true, simple);
    }
    public void DisableComponents(bool reenableCamera, bool simple)
    {
        if (reenableCamera)
            cam.SetCameraActive(true);
        content.SetActive(false);
        if (simple)
            common.SetActive(false);
        else
            special.SetActive(false);
    }

    public void EndQuestion()
    {
        if (countdown != null)
            StopCoroutine(countdown);
        canvas.TimesUp();
    }

	public void ApplyPenalty()
	{
        time -= 5;
        time = Mathf.Max(time, 0);
        timer.text = time >= 10 ? time.ToString() : numStrings[time];
    }

    public void ConcludeMinigame()
    {
        if (countdown != null)
            StopCoroutine(countdown);
        conclusion.Invoke();
    }

    public void StartMatchMinigame()
    {
        EnableComponents(false);
        matchMinigame.LoadMinigame();
        audioCtrl.TransitionToSoundtrack(SoundtrackType.Minigame, 1);
    }

    public void EndMatchMinigame()
    {
        audioCtrl.TransitionToSoundtrack(SoundtrackType.Level, 1);
        DisableComponents(false);
        canvas.NextTurn();
    }

    public void StartMinigame(TileMinigame originTile)
    {
        EnableComponents();
        curTile = originTile;
        curMinigame = minigames[Random.Range(0, minigames.Length)];
        curMinigame.SetActive(true);
        curMinigame.GetComponent<IMinigameLoader>().LoadMinigame();
        conclusion = EndMinigame;
    }

    public IEnumerator StartCountdown()
    {
        time = 30;
        timer.text = numStrings[time];
        while (time > 0)
        {
            time--;
            timer.text = numStrings[time];
            yield return new WaitForSeconds(1);
        }
    }
}