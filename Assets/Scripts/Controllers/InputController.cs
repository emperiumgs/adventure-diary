using UnityEngine;
using System.Collections;

public interface IInputReceiver
{
    void ReceiveInput(Camera eventCam);
}

public class InputController : MonoBehaviour
{
    public Camera[] cams;

    const int MAX_DIST = 25;

    Ray ray;
#if !UNITY_EDITOR && UNITY_ANDROID
    Touch t;
#endif
    Vector3 refPos;
    RaycastHit hit;
    int interactiveMask;

    void Awake()
    {
        interactiveMask = (1 << LayerMask.NameToLayer("Interactive")) | (1 << LayerMask.NameToLayer("Dice"));
    }

    void Update()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began)
            refPos = t.position;
        else
            refPos = Vector3.zero;
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        refPos = Input.GetMouseButtonDown(0) ? Input.mousePosition : Vector3.zero;
#endif
        if (refPos == Vector3.zero)
            return;

        foreach (Camera cam in cams)
        {
            if (cam.gameObject.activeInHierarchy)
            {
                ray = cam.ScreenPointToRay(refPos);
                if (Physics.Raycast(ray, out hit, MAX_DIST, interactiveMask))
                {
                    IInputReceiver ir = hit.collider.GetComponentInParent<IInputReceiver>();
                    if (ir != null)
                        ir.ReceiveInput(cam);
                }
            }
        }
    }
}