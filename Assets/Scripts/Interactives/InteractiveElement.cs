using UnityEngine;
using System.Collections;

public abstract class InteractiveElement : MonoBehaviour, IInputReceiver
{
    protected bool interactive;

    public void EnableInteraction()
    {
        interactive = true;
    }

    public void DisableInteraction()
    {
        interactive = false;
    }

    public abstract void ReceiveInput(Camera eventCam);
}