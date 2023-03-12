using UnityEngine;
using System.Collections;

public class AddMovementOnStateEnter : StateMachineBehaviour {

    public Vector3 direction;
    public float duration;
    public float speed;

    LastStand lastStand;

     override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	
        if(lastStand == null)
        {
            lastStand = animator.transform.GetComponent<LastStand>();
        }

        if (lastStand == null)
            return;

        lastStand.AddMovement(direction, duration,speed);

	}

}
