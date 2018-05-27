using UnityEngine;
using System.Collections;

public class Dice : MonoBehaviour, IInputReceiver
{
    public delegate void Next(int result);

    public GameObject diceCamera;
    public int rotationSpeed = 1000;

    MatchManager manager;
    Coroutine routine;
    AudioSource source;
    Vector3[] startRotations = {new Vector3(25,35,25), new Vector3(25, 35, 115), new Vector3(145, 0, 130), new Vector3(330, 0, 130), new Vector3(-40, 115, 220), new Vector3(145, 115, 310) };
    // By lowest to highest number
	Vector3[] rotations = { Vector3.zero, Vector3.forward * 90, Vector3.right * 90, Vector3.left * 90, Vector3.back * 90, Vector3.forward * 180 };
    bool done;

    void Awake()
    {
        manager = MatchManager.instance;
        source = GetComponent<AudioSource>();
    }

    public void ActivateDice(Next next)
    {
        gameObject.SetActive(true);
		routine = StartCoroutine(DiceRoutine(next));
    }

    public void DeactivateDice()
    {
        gameObject.SetActive(false);
        if (routine != null)
            StopCoroutine(routine);
    }

    public void ReceiveInput(Camera eventCam)
    {
        done = true;
    }

	public IEnumerator DiceRoutine(Next next)
	{
        transform.localEulerAngles = startRotations[Random.Range(0, startRotations.Length)];
        diceCamera.SetActive(true);
        done = false;
        while (!done)
            yield return null;
        source.Play();
        float time = 0;
		while (time < 1)
		{
			time += Time.deltaTime;
			transform.Rotate(Vector3.one * Time.deltaTime * rotationSpeed);
			yield return null;
		}
        int num = Random.Range(0, rotations.Length);
        transform.localEulerAngles = rotations[num];
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
        diceCamera.SetActive(false);
        routine = null;
        num = manager.curPlayer.doubled ? (num + 1) * 2 : num + 1;
        if (next != null)
            next(num);
    }
}