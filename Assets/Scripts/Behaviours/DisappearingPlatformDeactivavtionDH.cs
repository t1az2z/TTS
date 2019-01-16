using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatformDeactivavtionDH : StateMachineBehaviour {

    float deactivationTime;
    float deactivationTimeInitial;
    GameObject dp;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetFloat("CycleOffset", Random.Range(0f, 1f));
        dp = animator.gameObject;
        deactivationTime = animator.GetFloat("DeactTime");
        deactivationTimeInitial = deactivationTime;
        animator.GetComponent<DisappearingPlatform>().particles.Play();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        deactivationTime -= Time.deltaTime;
        animator.SetFloat("DeactTime", deactivationTime);
	}

	//OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetFloat("DeactTime", deactivationTimeInitial);
    }

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
