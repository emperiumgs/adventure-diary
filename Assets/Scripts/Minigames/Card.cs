using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour, IInputReceiver
{
    [HideInInspector]
    public AnimationCurve curve;
    [HideInInspector]
    public AudioSource source;
    [HideInInspector]
    public AudioClip flipUp,
        flipDown;
    public bool solution = false;
    [HideInInspector]
    public bool moving;

    const float CARD_MOVEMENT_TIME = 1.5f;

    CardsMinigame minigame;
    bool interactive;

    void Awake()
    {
        minigame = GetComponentInParent<CardsMinigame>();
    }

    public void ReceiveInput(Camera eventCam)
    {
        if (interactive)
            StartCoroutine(RevealCard());
    }

    public void Enable()
    {
        interactive = true;
    }

    public void Disable()
    {
        interactive = false;
    }

    public void ShowFront()
    {
        if (!source.isPlaying)
            source.PlayOneShot(flipUp);
        transform.localEulerAngles = Vector3.zero;
    }

    public void ShowBack()
    {
        if (!source.isPlaying)
            source.PlayOneShot(flipDown);
        transform.localEulerAngles = Vector3.forward * 180;
    }

    public void SwitchWith(Card other)
    {
        Vector3 otherPos = other.transform.position;
        other.SetMoveTo(transform.position);
        SetMoveTo(otherPos);
    }

    public Coroutine SetMoveTo(Vector3 pos)
    {
        moving = true;
        return StartCoroutine(MoveTo(pos));
    }

    IEnumerator MoveTo(Vector3 pos)
    {
        Vector3 startPos = transform.position,
            dist = pos - startPos;
        float time = 0;
        while (time < CARD_MOVEMENT_TIME)
        {
            time += Time.deltaTime;
            transform.position = startPos + dist * curve.Evaluate(time / CARD_MOVEMENT_TIME);
            yield return null;
        }
        transform.position = pos;
        moving = false;
        minigame.CardReach();
    }

    IEnumerator RevealCard()
    {
        ShowFront();
        minigame.DisableAllCards();
        yield return new WaitForSeconds(1);
        minigame.Solution(solution);
    }
}