using UnityEngine;
using System.Collections;

public class GameCamera : Singleton<GameCamera>
{
    public AnimationCurve movementCurve;

    public Transform menuSocket,
        bookSocket,
        levelSocket;

    const int BOOK_MOVEMENT_TIME = 5;

    Transform pivot,
        target;
    Camera cam;
    float speed = 5;

    void Awake()
    {
        cam = GetComponent<Camera>();
        pivot = transform.parent;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        pivot.position = Vector3.Lerp(pivot.position, target.position, speed * Time.deltaTime);
    }

    public void SetCameraActive(bool value)
    {
        cam.enabled = value;
    }

    public Coroutine ToMenuView()
    {
        return RedirectTo(menuSocket, BOOK_MOVEMENT_TIME);
    }

    public Coroutine ToBookView()
    {
        return RedirectTo(bookSocket, BOOK_MOVEMENT_TIME);
    }

    public Coroutine ToLevelView(float time)
    {
        return RedirectTo(levelSocket, time);
    }

    public Coroutine RedirectTo(Transform redirectTarget, float movementTime)
    {
        return StartCoroutine(RedirectRoutine(redirectTarget, movementTime));
    }

    IEnumerator RedirectRoutine(Transform redirectTarget, float movementTime)
    {
        target = null;
        float time = 0;
        Vector3 targetPos = redirectTarget.position,
            startPos = pivot.position,
            dir = targetPos - startPos,
            targetRotation = new Vector3(redirectTarget.localEulerAngles.x, 0),
            startRotation = transform.localEulerAngles,
            diff = targetRotation - startRotation;
        bool rotate = targetRotation != Vector3.zero && diff != Vector3.zero;
        while (time < movementTime)
        {
            time += Time.deltaTime;
            pivot.position = startPos + dir * movementCurve.Evaluate(time / movementTime);
            if (rotate)
                transform.localEulerAngles = startRotation + diff * movementCurve.Evaluate(time / movementTime);
            yield return null;
        }
        target = redirectTarget;
    }
}