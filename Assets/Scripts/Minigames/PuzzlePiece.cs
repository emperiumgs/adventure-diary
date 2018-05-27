using UnityEngine;
using System.Collections;

public class PuzzlePiece : MonoBehaviour, IInputReceiver
{
    public bool solution = false;
    [HideInInspector]
    public PuzzleMinigame puzzle;

    Coroutine routine;
    Vector3 pivotPos;
    bool attached;

    Vector3 inputPos
    {
        get
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Touch t = Input.GetTouch(0);
            return t.position;
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return Input.mousePosition;
#endif
        }
    }
    bool inputActive
    {
        get
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Touch t = Input.GetTouch(0);
            return t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            return Input.GetMouseButton(0);
#endif
        }
    }

    void Start()
    {
        pivotPos = transform.position;
    }

    public void ReceiveInput(Camera cam)
    {
        puzzle.Solution(this);
        //routine = StartCoroutine(InputMovement(cam));
    }

    public void Attach(Vector3 pos)
    {
        attached = true;
        transform.position = pos;
    }

    public void Cancel()
    {
        if (routine != null)
            StopCoroutine(routine);
        gameObject.SetActive(false);
    }

    IEnumerator InputMovement(Camera cam)
    {
        Vector3 newPos,
            aux;
        while (!attached && inputActive)
        {
            aux = inputPos;
            aux.z = cam.transform.position.y - transform.position.y;
            newPos = cam.ScreenToWorldPoint(aux);
            newPos.y = transform.position.y;
            transform.position = newPos;
            yield return null;
        }
        if (!attached)
            StartCoroutine(ResetPos());
        routine = null;
    }

    IEnumerator ResetPos()
    {
        while (Vector3.Distance(transform.position, pivotPos) > .1f)
        {
            transform.position = Vector3.Lerp(transform.position, pivotPos, 5 * Time.deltaTime);
            yield return null;
        }
    }
}