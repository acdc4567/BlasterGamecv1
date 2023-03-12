using UnityEngine;
using System.Collections;

public class CustomAnimationCurve_AnimBehaviour : StateMachineBehaviour {

    public AnimationCurve curve;
    public string floatName;
    float timer = 0;
    public float test1;
	
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer++;
        curve.preWrapMode = WrapMode.Loop;
        timer += Time.deltaTime;

        float value = curve.Evaluate(timer);
        animator.SetFloat(floatName,value );
        Debug.Log(curve.Evaluate(timer));
        curve.postWrapMode = WrapMode.Loop;
	}

	
}
