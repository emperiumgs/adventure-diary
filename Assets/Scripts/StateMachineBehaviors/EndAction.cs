using UnityEngine;
using System.Collections;

public abstract class EndAction : StateMachineBehaviour
{
    bool messageSent;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        messageSent = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!messageSent && stateInfo.normalizedTime >= 1)
        {
            messageSent = true;
            Action();
        }
    }

    public abstract void Action();
}