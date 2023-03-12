using UnityEngine;
using System.Collections;

public class RiotShield : MonoBehaviour {

    Animator anim;
    StateManager states;

    public Transform leftShoulder;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        states = GetComponentInParent<StateManager>();
    }


    void OnEnable()
    {
        if(states == null)
            states = GetComponentInParent<StateManager>();

        states.dontRun = true;
    }

    void OnDisable()
    {
        states.dontRun = false;
    }

	void Update () 
    {
        if (leftShoulder == null)
            leftShoulder = transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftShoulder);

        transform.position = leftShoulder.position;

        anim.SetBool("Aim", states.aiming);
	}
}
