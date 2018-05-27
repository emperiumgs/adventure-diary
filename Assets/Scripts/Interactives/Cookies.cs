using UnityEngine;
using System.Collections;

public class Cookies : MonoBehaviour, IPopupActor, IPopupReactor
{
    GameObject[] children;
    Vector3[] startPos;

    void Awake()
    {
        children = new GameObject[transform.childCount];
        startPos = new Vector3[children.Length];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = transform.GetChild(i).gameObject;
            startPos[i] = children[i].transform.position;
        }
    }

    public void PopupPull()
    {
        foreach (GameObject child in children)
            child.SetActive(true);
    }

    public void PopupRestart()
    {
        StartCoroutine(Restart());
    }

    IEnumerator Restart()
    {
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            for (int i = 0; i < children.Length; i++)
                children[i].transform.localScale = Vector3.one/2 + (Vector3.one/2 * (1 - (time / 1)));
            yield return null;
        }
        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetActive(false);
            children[i].transform.position = startPos[i];
            children[i].transform.localScale = Vector3.one;
        }
    }
}