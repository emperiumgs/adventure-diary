using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public interface IPopupActor
{
    void PopupPull();
}
public interface IPopupReactor
{
    void PopupRestart();
}

public class Popup : InteractiveElement
{
    public enum PopDirections
    {
        Up,
        Right,
        Down,
        Left
    }

    public enum FadeFlags
    {
        None = 0,
        ToWhite = 1,
        Interactive = 2,
        DelayedInvoke = 4
    }

    public PopDirections direction;
    public UnityEvent onPull,
        onRestart;
    public bool startInactive,
        delayed,
        looping;
    public float loopTime;

    const float SWIPE_REQ = 2,
        SWIPE_INTENSITY = 25,
        SWIPE_LIMIT = .35f,
        FADE_TIME = .5f;

    static AudioController audioCtrl;

    MeshRenderer rend;
    BoxCollider col;
    Vector3 startPos,
        tPos,
        finalPos,
        limitPos,
        prevMouse;
    float screenWidth = Screen.width,
        screenHeight = Screen.height,
        moveAmount;
    bool pulling;

    Vector2 inputPos
    {
        get
        {
            if (Input.touchSupported)
            {
                Touch t = Input.GetTouch(0);
                return t.position;
            }
            else
                return Input.mousePosition;
        }
    }
    Vector2 prevPos
    {
        get
        {
            if (Input.touchSupported)
            {
                Touch t = Input.GetTouch(0);
                return t.position - t.deltaPosition;
            }
            else
                return prevMouse;
        }
    }
    bool inputActive
    {
        get
        {
            if (Input.touchSupported)
            {
                Touch t = Input.GetTouch(0);
                return t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            }
            else
                return Input.GetMouseButton(0);
        }
    }

    void Awake()
    {
        rend = GetComponentInChildren<MeshRenderer>();
        col = GetComponentInChildren<BoxCollider>();
        startPos = tPos = transform.position;
        if (audioCtrl == null)
            audioCtrl = AudioController.instance;
        Vector3 mod = Vector3.zero;
        switch (direction)
        {
            case PopDirections.Up:
                mod = Vector3.forward;
                break;
            case PopDirections.Right:
                mod = Vector3.right;
                break;
            case PopDirections.Down:
                mod = Vector3.back;
                break;
            case PopDirections.Left:
                mod = Vector3.left;
                break;
        }
        limitPos = transform.position - mod * SWIPE_LIMIT;
        mod *= SWIPE_REQ;
        finalPos = transform.position + mod;
        if (startInactive)
        {
            col.enabled = interactive;
            rend.material.color *= new Color(1, 1, 1, 0);
            rend.enabled = interactive;
        }
        else
            interactive = true;
    }

    void UpdateSwipe()
    {
        if (direction == PopDirections.Right || direction == PopDirections.Left)
        {
            moveAmount = (inputPos.x - prevPos.x) / screenWidth;
            moveAmount *= finalPos.x - startPos.x;
            moveAmount *= SWIPE_INTENSITY;
            moveAmount *= direction == PopDirections.Right ? 1 : -1;
            tPos = new Vector3(tPos.x + moveAmount, tPos.y, tPos.z);
            if (direction == PopDirections.Right)
            {
                if (tPos.x > finalPos.x)
                    SuccessPull();
                else if (tPos.x < limitPos.x)
                    FailPull();
            }
            else
            {
                if (tPos.x < finalPos.x)
                    SuccessPull();
                else if (tPos.x > limitPos.x)
                    FailPull();
            }
        }
        else
        {
            moveAmount = (inputPos.y - prevPos.y) / screenHeight;
            moveAmount *= finalPos.z - startPos.z;
            moveAmount *= SWIPE_INTENSITY;
            moveAmount *= direction == PopDirections.Up ? 1 : -1;
            tPos = new Vector3(tPos.x, tPos.y, tPos.z + moveAmount);
            if (direction == PopDirections.Up)
            {
                if (tPos.z > finalPos.z)
                    SuccessPull();
                else if (tPos.z < limitPos.z)
                    FailPull();
            }
            else
            {
                if (tPos.z < finalPos.z)
                    SuccessPull();
                else if (tPos.z > limitPos.z)
                    FailPull();
            }
        }
        prevMouse = inputPos;
    }

    void SuccessPull()
    {
        pulling = false;
        interactive = false;
        col.enabled = interactive;
        audioCtrl.source.PlayOneShot(audioCtrl.popup);
        StartCoroutine(Fade(FadeFlags.DelayedInvoke));
        if (looping)
            StartCoroutine(Restart());
        if (!delayed)
            onPull.Invoke();
    }

    void FailPull()
    {
        pulling = false;
    }

    public override void ReceiveInput(Camera eventCam)
    {
        if (interactive)
            SuccessPull();
            //StartCoroutine(Pull());
    }

    public void Disappear()
    {
        if (rend.enabled)
            StartCoroutine(Fade(FadeFlags.None));
    }

    public void Reappear()
    {
        if (!rend.enabled)
            StartCoroutine(Fade(FadeFlags.ToWhite | FadeFlags.Interactive));
    }

    IEnumerator Pull()
    {
        pulling = true;
        prevMouse = inputPos;
        yield return null;
        while (pulling && inputActive)
        {
            UpdateSwipe();
            Debug.DrawLine(finalPos, tPos, Color.cyan, Time.deltaTime, false);
            Debug.DrawLine(limitPos, startPos, Color.yellow, Time.deltaTime, false);
            yield return null;
        }
        tPos = startPos;
    }

    public IEnumerator Fade(FadeFlags flags)
    {
        bool toWhite = (flags & FadeFlags.ToWhite) == FadeFlags.ToWhite;
        if (toWhite)
            rend.enabled = true;
        Color current = rend.material.color,
            target = current;
        target.a = toWhite ? 1 : 0;
        float time = 0;
        while (time < FADE_TIME)
        {
            current = rend.material.color;
            current.a = toWhite ? time / FADE_TIME : 1 - (time / FADE_TIME);
            rend.material.color = current;
            time += Time.deltaTime;
            yield return null;
        }
        rend.material.color = target;
        if (toWhite && (flags & FadeFlags.Interactive) == FadeFlags.Interactive)
        {
            interactive = true;
            col.enabled = interactive;
        }
        else
        {
            rend.enabled = false;
            if ((flags & FadeFlags.DelayedInvoke) == FadeFlags.DelayedInvoke && delayed)
                onPull.Invoke();
        }
    }

    public IEnumerator Restart()
    {
        yield return new WaitForSeconds(loopTime);
        yield return StartCoroutine(Fade(FadeFlags.ToWhite | FadeFlags.Interactive));
        onRestart.Invoke();
    }
}