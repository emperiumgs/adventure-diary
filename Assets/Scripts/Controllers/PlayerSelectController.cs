using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class PlayerSelectController : PageController
{
    public Transform socket;
    public GameObject[] prefabs;
    public InteractiveCharacter[] chars;
    public Transform[] btns;
    public InputField[] inputs;
    public Popup prev;

    public bool interacted
    {
        get { return choices != 0; }
    }

    AudioController audioCtrl;
    Animator curChoice;
    int reqChoices,
        choices;

    protected override void Awake()
    {
        base.Awake();
        audioCtrl = AudioController.instance;
    }

    public override void LoadPage()
    {
        nextPopup.onPull.AddListener(SwitchSoundtrack);
        nextPopup.onPull.AddListener(HideCharacters);
        prev.onPull.AddListener(HideCharacters);
        nextPopup.onPull.AddListener(RemoveListeners);
        prev.onPull.AddListener(RemoveListeners);
        reqChoices = manager.players.Length;
        choices = 0;
        ReloadPage();
        ShowCharacters();
    }

    public void ReloadPage()
    {
        EnableCharacters();
        foreach (InputField i in inputs)
        {
            i.text = "";
            i.interactable = true;
            i.gameObject.SetActive(false);
        }
        Animator[] allChars = socket.GetComponentsInChildren<Animator>();
        if (allChars.Length > chars.Length)
        {
            foreach (Animator a in allChars)
                if (!a.GetComponent<Collider>())
                    Destroy(a.gameObject);
        }
    }

    public void SelectCharacter(int index)
    {
        if (choices == reqChoices)
            return;
        foreach (InteractiveCharacter c in chars)
            c.DisableInteraction();
        GameObject newChar = Instantiate(prefabs[index]);
        newChar.transform.SetParent(socket);
        newChar.transform.position = btns[choices].transform.position;
        newChar.transform.rotation = chars[0].transform.rotation;
        curChoice = newChar.GetComponent<Animator>();
        inputs[choices].gameObject.SetActive(true);
        inputs[choices].Select();
    }

    public void ConfirmName()
    {
        manager.ConfirmPlayer(inputs[choices].text, choices, curChoice);
        inputs[choices].interactable = false;
        curChoice = null;
        choices++;
        EnableCharacters();
        if (choices == reqChoices)
            nextPopup.Reappear();
    }

    public void EnableCharacters()
    {
        foreach (InteractiveCharacter c in chars)
            c.EnableInteraction();
    }

    public void ShowCharacters()
    {
        socket.gameObject.SetActive(true);
    }

    public void HideCharacters()
    {
        socket.gameObject.SetActive(false);
    }

    public void RemoveListeners()
    {
        nextPopup.onPull.RemoveListener(SwitchSoundtrack);
        nextPopup.onPull.RemoveListener(HideCharacters);
        prev.onPull.RemoveListener(HideCharacters);
    }

    public void SwitchSoundtrack()
    {
        audioCtrl.TransitionToSoundtrack(SoundtrackType.Level, 5);
    }
}