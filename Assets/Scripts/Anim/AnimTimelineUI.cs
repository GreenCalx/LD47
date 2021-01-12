using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTimelineUI : StateMachineBehaviour
{
    public readonly int MAX_TIME = 25; 
    private const string __blendTreeKeyParm = "Blend";
    private const string __timeParm = "time";
    private const string __updateAnimParm = "update_anim";

    private float __time;
    private bool __updateAnimRequest = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        __updateAnimRequest = false;
        freeze_anim(animator);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool (__updateAnimParm, __updateAnimRequest);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        updateBlendTreeValue(animator);
        __updateAnimRequest = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Implement code that processes and affects root motion
        updateBlendTreeValue(animator);
    }

    private void updateBlendTreeValue(Animator animator)
    {
        float blend = 0f;

        blend = __time / MAX_TIME;
        animator.SetFloat( __blendTreeKeyParm, blend);
    }

    public void updateTime( int iNewTime )
    {
        if ( iNewTime == __time )
            return;

        __time = (float)iNewTime;
        __updateAnimRequest = true;
        
    }

    public void freeze_anim(Animator animator)
    {
        
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
