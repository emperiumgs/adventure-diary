using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InteractiveCharacter : InteractiveElement
{
    public UnityEvent onClick;

    public override void ReceiveInput(Camera eventCam)
    {
        if (interactive)
            onClick.Invoke();
    }
}