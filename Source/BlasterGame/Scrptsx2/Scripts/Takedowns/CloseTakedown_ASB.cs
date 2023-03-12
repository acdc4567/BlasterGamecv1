using UnityEngine;
using System.Collections;

public class CloseTakedown_ASB : StateMachineBehaviour {

 

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        
        if(animator.GetComponent<TakedownPlayer>())
        {
            animator.GetComponent<TakedownPlayer>().EndTakedown();
        }
	
	}



}
