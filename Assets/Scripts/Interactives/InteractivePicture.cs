using UnityEngine;
using UnityEngine.Events;

public class InteractivePicture : InteractiveElement
{
    public UnityEvent onClick;
    public int value;

    static Color selected = new Color(.5f, .75f, 1f);

    MeshRenderer mesh;
    Color origColor;

    void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        origColor = mesh.material.color;
    }

    public void Select()
    {
        mesh.material.color = selected;
    }

    public void Deselect()
    {
        mesh.material.color = origColor;
    }

    public override void ReceiveInput(Camera eventCam)
    {
        if (!interactive)
            return;
        Select();
        onClick.Invoke();
    }
}