using UnityEngine;
using System.Collections;

public class BookController : Singleton<BookController>
{
    enum Pages
    {
        PlayerCount,
        PlayerSelect,
        StartMatch
    }

    public Animator book;
    public GameObject[] pages;
    public GameObject menu;
    public Popup[] menuPopups;
    public GameObject cookies;
    public GameObject pageCtrls;
    public Popup[] pagePopups;

    const int PAGE_MOVEMENT_TIME = 5;

    PlayerCountController countPage;
    PlayerSelectController selectPage;
    MatchManager manager;
    MatchCanvas canvas;
    GameCamera cam;
    AudioController audioCtrl;
    Animator curPage,
       nextPage,
       prevPage;

    int curPageIndex
    {
        get
        {
            int i = 0;
            bool found = false;
            if (curPage == null)
                return -1;
            while (!found && i < pages.Length)
            {
                if (i >= pages.Length || curPage.gameObject == pages[i] || curPage.GetComponentInParent<PlayerCountController>() != null)
                    found = true;
                if (!found)
                    i++;
            }
            return i;
        }
    }

    void Awake()
    {
        manager = MatchManager.instance;
        cam = GameCamera.instance;
        canvas = MatchCanvas.instance;
        audioCtrl = AudioController.instance;
        countPage = pages[0].GetComponent<PlayerCountController>();
        nextPage = countPage.slider;
        selectPage = pages[1].GetComponent<PlayerSelectController>();
        countPage.nextPopup = selectPage.nextPopup = pagePopups[0];
    }

    void UpdateBookElements()
    {
        switch (curPageIndex)
        {
            case (int)Pages.PlayerCount:
                cookies.SetActive(false);
                pageCtrls.SetActive(true);
                menu.SetActive(false);
                curPage = countPage.EnableOpen();
                countPage.LoadPage();
                pagePopups[1].Reappear();
                if (countPage.chosen)
                    pagePopups[0].Reappear();
                if (selectPage.interacted)
                    selectPage.ReloadPage();
                break;
            case (int)Pages.PlayerSelect:
                selectPage.LoadPage();
                pagePopups[1].Reappear();
                break;
            case (int)Pages.StartMatch:
                if (manager.round == 0)
                {
                    cam.ToLevelView(PAGE_MOVEMENT_TIME / 2);
                    manager.ConfigureTurnOrder();
                }
                break;
        }
    }

    public void OpenBook(Popup puller)
    {
        StartCoroutine(OpenBookRoutine(puller));
    }

    #region PageEvents
    public void ConcludeOpenNextPage()
    {
        if (prevPage != null && curPageIndex < pages.Length - 2)
            prevPage.gameObject.SetActive(false);
        if (curPage != null)
            prevPage = curPage;
        curPage = nextPage;
        if (curPageIndex + 1 < pages.Length)
        {
            nextPage = pages[curPageIndex + 1].GetComponentInChildren<Animator>(true);
            nextPage.gameObject.SetActive(true);
        }
        else
            nextPage = null;
        UpdateBookElements();
    }

    public void ConcludeOpenPrevPage()
    {
        if (nextPage != null)
            nextPage.gameObject.SetActive(false);
        nextPage = curPage;
        curPage = prevPage;
        if (curPageIndex - 1 > 0 && pages[curPageIndex - 1] != null)
        {
            prevPage = pages[curPageIndex - 1].GetComponentInChildren<Animator>(true);
            prevPage.gameObject.SetActive(true);
        }
        else
            prevPage = null;
        UpdateBookElements();
    }
    #endregion

    public void RemoveInactivePages()
    {
        Destroy(countPage.gameObject);
        Destroy(selectPage.gameObject);
    }

    public void ToNextPage()
    {
        pagePopups[1].Disappear();
        OpenNextPage();
    }

    public void ToPreviousPage()
    {
        pagePopups[0].Disappear();
        if (curPageIndex == 0)
        {
            StartCoroutine(CloseBookRoutine());
            return;
        }
        OpenPreviousPage();
    }

    public Coroutine OpenNextPage()
    {
        curPage.SetTrigger("next");
        nextPage.SetTrigger("next");
        audioCtrl.source.PlayOneShot(audioCtrl.book);
        return StartCoroutine(MovePages(true));
    }

    public Coroutine OpenPreviousPage()
    {
        curPage.SetTrigger("prev");
        prevPage.SetTrigger("prev");
        audioCtrl.source.PlayOneShot(audioCtrl.book);
        return StartCoroutine(MovePages(false));
    }

    #region BookRoutines
    IEnumerator OpenBookRoutine(Popup puller)
    {
        for (int i = 0; i < menuPopups.Length; i++)
        {
            if (menuPopups[i] != puller)
            {
                if (i == menuPopups.Length - 1)
                    yield return StartCoroutine(menuPopups[i].Fade(Popup.FadeFlags.None));
                else
                    menuPopups[i].Disappear();
            }
        }
        book.SetTrigger("toggle");
        audioCtrl.source.PlayOneShot(audioCtrl.book);
        nextPage.SetTrigger("next");
        yield return cam.ToBookView();
        canvas.ToggleMenuButton();
    }

    IEnumerator CloseBookRoutine()
    {
        curPage = countPage.EnableSlider();
        book.SetTrigger("toggle");
        audioCtrl.source.PlayOneShot(audioCtrl.book);
        curPage.SetTrigger("prev");
        nextPage = curPage;
        curPage = null;
        prevPage = null;
        cookies.SetActive(true);
        yield return cam.ToMenuView();
        pageCtrls.SetActive(false);
        menu.SetActive(true);
        foreach (Popup p in menuPopups)
            p.Reappear();
    }
    #endregion

    IEnumerator MovePages(bool open)
    {
        Transform current = curPage.transform,
            other = open ? nextPage.transform : prevPage.transform;
        float diff = other.position.y - current.position.y,
            amountY,
            offsetX = open ? -.3f : .3f,
            amountX = 0;
        Vector3 auxVector,
            finalCurrent = other.position,
            finalOther = current.position;
        float time = 0;
        while (time < PAGE_MOVEMENT_TIME && current != null)
        {
            time += Time.deltaTime;
            amountY = diff * Time.deltaTime / PAGE_MOVEMENT_TIME;
            amountX = offsetX * Time.deltaTime / PAGE_MOVEMENT_TIME;
            if (time > PAGE_MOVEMENT_TIME / 2)
                amountX *= -1;
            auxVector = current.position;
            auxVector.y += amountY;
            auxVector.x += amountX;
            auxVector.x = open ? Mathf.Clamp(auxVector.x, offsetX, 0) : Mathf.Clamp(auxVector.x, 0, offsetX);
            current.position = auxVector;
            auxVector = other.position;
            auxVector.y -= amountY;
            auxVector.x -= amountX;
            auxVector.x = open ? Mathf.Clamp(auxVector.x, 0, -offsetX) : Mathf.Clamp(auxVector.x, -offsetX, 0);
            other.position = auxVector;
            yield return null;
        }
        if (current != null)
            current.position = finalCurrent;
        other.position = finalOther;
    }
}