using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardsMinigame : MonoBehaviour, IMinigameLoader
{
    public AnimationCurve movementCurve;
    public AudioClip flipUp,
        flipDown;

    MinigameController ctrl;
    MatchManager manager;
    MatchCanvas canvas;
    AudioSource source;
    Vector3[] positions;
    Card[] cards;

    const int MAX_CARD_SHUFFLES = 3;

    int shuffles,
        cardsReached;

    void Awake()
    {
        ctrl = MinigameController.instance;
        manager = MatchManager.instance;
        canvas = MatchCanvas.instance;
        source = GetComponent<AudioSource>();
        cards = GetComponentsInChildren<Card>();
        positions = new Vector3[cards.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = cards[i].transform.position;
            if (i % 2 != 0)
                positions[i] += Vector3.up * .1f;
            cards[i].curve = movementCurve;
            cards[i].source = source;
            cards[i].flipUp = flipUp;
            cards[i].flipDown = flipDown;
        }
    }

    public void LoadMinigame()
    {
        shuffles = 0;
        List<Vector3> collection = new List<Vector3>(positions.Length);
        collection.AddRange(positions);
        Vector3 pos;
        for (int i = 0; i < cards.Length; i++)
        {
            pos = collection[Random.Range(0, collection.Count)];
            cards[i].transform.position = pos;
            cards[i].ShowFront();
            cards[i].Disable();
            collection.Remove(pos);
        }
        StartCoroutine(ShuffleCards());
    }

    public void CardReach()
    {
        cardsReached++;
        if (cardsReached == cards.Length)
            shuffles++;
    }

    public void EnableAllCards()
    {
        foreach (Card c in cards)
            c.Enable();
    }

    public void DisableAllCards()
    {
        foreach (Card c in cards)
            c.Disable();
    }

    public void ShowAllCards()
    {
        foreach (Card c in cards)
            c.ShowFront();
    }

    public void Solution(bool value)
    {
        DisableAllCards();
        if (value)
        {
            source.PlayOneShot(canvas.right);
            manager.MinigameWon();
        }
        else
            source.PlayOneShot(canvas.wrong);
        StartCoroutine(DelayedConclusion());
    }

    IEnumerator ShuffleCards()
    {
        yield return new WaitForSeconds(3);
        foreach (Card card in cards)
            card.ShowBack();
        List<Card> collection = new List<Card>(cards.Length);
        bool done;
        int i,
            c,
            r = 0,
            curShuffle;
        while (shuffles < MAX_CARD_SHUFFLES)
        {
            yield return new WaitForSeconds(.5f);
            // Get card positions
            cardsReached = 0;
            collection.Clear();
            for (i = 0; i < positions.Length; i++)
            {
                c = 0;
                done = false;
                while (!done && c < cards.Length)
                {
                    if (cards[c].transform.position == positions[i])
                    {
                        collection.Add(cards[c]);
                        done = true;
                    }
                    c++;
                }
            }
            // Assign switch positions
            for (i = 0; i < collection.Count; i++)
            {
                if (!collection[i].moving)
                {
                    if (i < 2)
                        r = Random.Range(0, 2);
                    switch (i)
                    {
                        case 0:
                        case 1:
                            if (r == 0)
                                collection[i].SwitchWith(collection[i + 1]);
                            else
                                collection[i].SwitchWith(collection[i + 3]);
                            break;
                        case 2:
                            collection[i].SwitchWith(collection[i + 3]);
                            break;
                        case 3:
                        case 4:
                            collection[i].SwitchWith(collection[i + 1]);
                            break;
                    }
                }
            }
            curShuffle = shuffles;
            yield return new WaitUntil(()=> curShuffle != shuffles);
        }
        EnableAllCards();
    }

    IEnumerator DelayedConclusion()
    {
        yield return new WaitForSeconds(1);
        ShowAllCards();
        yield return new WaitForSeconds(1);
        ctrl.ConcludeMinigame();
    }
}