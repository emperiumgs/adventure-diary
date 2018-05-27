using UnityEngine;

public class PlayerCountController : PageController
{
    public InteractivePicture[] buttons;
    public Animator slider,
        open;
    public Renderer[] sliderPages,
        openPages;
    public bool chosen { get; private set; }

    int chosenCount;

    protected override void Awake()
    {
        base.Awake();
        DisablePictures();
        EnableSlider();
    }

    void ToggleSelected()
    {
        buttons[chosenCount - 1].Deselect();
    }

    void EnablePictures()
    {
        foreach (InteractivePicture p in buttons)
            p.EnableInteraction();
    }

    void DisablePictures()
    {
        foreach (InteractivePicture p in buttons)
            p.DisableInteraction();
    }

    public void ChooseCount(InteractivePicture obj)
    {
        if (chosen)
            ToggleSelected();
        else
        {
            chosen = true;
            nextPopup.Reappear();
        }
        chosenCount = obj.value;
    }

    public void ConfirmCount()
    {
        manager.SelectPlayers(chosenCount);
        nextPopup.onPull.RemoveListener(ConfirmCount);
        DisablePictures();
    }

    public Animator EnableSlider()
    {
        for (int i = 0; i < sliderPages.Length; i++)
        {
            sliderPages[i].enabled = true;
            openPages[i].enabled = false;
        }
        return slider;
    }

    public Animator EnableOpen()
    {
        for (int i = 0; i < openPages.Length; i++)
        {
            openPages[i].enabled = true;
            sliderPages[i].enabled = false;
        }
        return open;
    }

    public override void LoadPage()
    {
        nextPopup.onPull.AddListener(ConfirmCount);
        EnablePictures();
    }
}