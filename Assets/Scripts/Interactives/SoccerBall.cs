using UnityEngine;
using System.Collections;

public class SoccerBall : InteractiveElement
{
    const int FORCE_INTENSITY = 7;

    public int id;

    static AudioController audioCtrl;
    static BrazilMinigame bm;

    Rigidbody rb;
    bool moving;

    public Quaternion rotation
    {
        get { return transform.rotation; }
        set { transform.rotation = value; }
    }
    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    public bool isKinematic
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            return rb.isKinematic;
        }
        set
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            rb.isKinematic = value;
        }
    }

    void Awake()
    {
        if (bm == null)
        {
            bm = BrazilMinigame.instance;
            audioCtrl = AudioController.instance;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.GetComponent<SoccerBall>())
            audioCtrl.source.PlayOneShot(bm.ballCollide);
        if (!moving)
            Stop();
    }

    public void Shoot(float ratio)
    {
        audioCtrl.source.PlayOneShot(bm.ballShoot);
        isKinematic = false;
        moving = true;
        rb.AddRelativeForce(Vector3.forward * FORCE_INTENSITY * ratio, ForceMode.Impulse);
        audioCtrl.PlayLooping(bm.ballRoll);
        Stop();
    }

    public override void ReceiveInput(Camera eventCam)
    {
        if (interactive)
            bm.BallInput();
    }

    Coroutine Stop()
    {
        return StartCoroutine(StopMovement());
    }

    IEnumerator StopMovement()
    {
        bool callNext = moving;
        yield return new WaitForSeconds(3);
        audioCtrl.StopLooping();
        audioCtrl.source.PlayOneShot(bm.ballEnd);
        moving = false;
        isKinematic = true;
        yield return new WaitForFixedUpdate();
        isKinematic = false;
        if (callNext)
            bm.NextPlayer();
    }
}