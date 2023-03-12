using UnityEngine;
using System.Collections;

public class IKHandler : MonoBehaviour {

    Animator anim;
    StateManager states;

    public float lookWeight = 1;
    public float bodyWeight = 0.8f;
    public float headWeight = 1;
    public float clampWeight = 1;

    float targetWeight;

    public Transform weaponHolder;
    [HideInInspector]
    public Transform rightShoulder;
    public Transform overrideLookTarget;

    [HideInInspector]
    public Transform rightHandIkTarget;
    [HideInInspector]
    public Transform rightHandIKRotation;
    [HideInInspector]
    public Transform rightElbowTarget;
    public float rightHandIkWeight;
    float targetRHweight;

    [HideInInspector]
    public Transform leftElbowTarget;
    [HideInInspector]
    public Transform leftHandIkTarget;//same as above
    public float leftHandIKweight;
    float targetLHweight;

    Transform aimHelperRS;
    [HideInInspector]
    public Transform aimHelperLS;
    Transform leftHandHelper;

    [HideInInspector]
    public bool bypassAngleClamp;
    public bool disableIK;

    [HideInInspector]
    public bool LHIK_dis_notAiming;

    DualWield.Akimbo akimbo;

    public void Init() {
        aimHelperRS = new GameObject().transform;
        aimHelperRS.name = "Right Shoulder Aim Helper";

        aimHelperLS = new GameObject().transform;
        aimHelperLS.name = "Left Shoulder Aim Helper";

        leftHandHelper = new GameObject().transform;
        leftHandHelper.name = "Left hand helper";

        anim = GetComponent<Animator>();
        states = GetComponent<StateManager>();

        if (GetComponent<DualWield.Akimbo>())
        {
            akimbo = GetComponent<DualWield.Akimbo>();
            akimbo.Init(states);
        }
    }

    public void Tick()
    {
        disableIK = anim.GetBool("DisableIK");

        if (!states.meleeWeapon)
        {
            if(states.weaponManager.ReturnCurrentWeapon().onRunDisableIK)
            {
                if (!states.aiming)
                {
                    if (Mathf.Abs(states.horizontal) > 0.5f || Mathf.Abs(states.vertical) > 0.5f)
                    {
                        if(!states.dontRun)
                            disableIK = true;
                    }
                }
            }

            HandleShoulders();

            //I put everything into their own functions
            AimWeight();
            HandleRightHandIKWeight();

            if (akimbo == null)
            {
                HandleLeftHandIKWeight();
            }
            else
            {
                if (!akimbo.enableAkimbo)
                {
                    HandleLeftHandIKWeight();
                }
            }

            HandleShoulderRotation();
        }
        else
        {
            lookWeight = 0;
        }

        if(akimbo)
        {
            akimbo.Tick();
        }
    }
     
    void HandleShoulders()
    {
        //same as before
        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        else
        {
            weaponHolder.position = rightShoulder.position;
        }

    }

    void AimWeight()
    {
        if (states.aiming && !states.reloading && !states.vaulting && states.onGround)
        {
            Vector3 directionTowardsTarget = aimHelperRS.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionTowardsTarget);

            if (angle < 90 || bypassAngleClamp)
            {
                targetWeight = 1;
            }
            else
            {
                targetWeight = 0;
            }
        }
        else
        {
            targetWeight = 0;
        }

        float multiplier = (states.aiming) ? 5 : 30;

        if(disableIK)
        {
            lookWeight = 0;
            targetWeight = 0;
        }

        lookWeight = Mathf.Lerp(lookWeight, targetWeight,
            states.myDelta * multiplier);
    }

    public void AkimboStatus(bool status)
    {
        if (akimbo)
        {
            if (status)
                akimbo.EnableAkimbo();
            else
                akimbo.DisableAkimbo();
        }
        else
        {
            if (GetComponent<DualWield.Akimbo>())
            {
                akimbo = GetComponent<DualWield.Akimbo>();
                akimbo.Init(states);
            }

            if(akimbo == null)
                return;
        }
    }

    void HandleRightHandIKWeight()
    {
        float multiplier = 3;

        if(states.inCover) //if we are in cover
        {       
            targetRHweight = 0;//we don't want ik for the right hand

            if (states.aiming)//unless we are aiming
            {
                targetRHweight = 1;
                multiplier = 2;
            }
            else
            {
                multiplier = 10;
            }
        }
        else//if we are not
        {
            //then control the ik the same way as the look weight
            rightHandIkWeight = lookWeight;
        }

        if (states.reloading)//if we are reloading
        {
            //we don't want ik
            targetRHweight = 0;
            multiplier = 5;
        }

        //lerp to the desired values
        rightHandIkWeight = Mathf.Lerp(rightHandIkWeight, targetRHweight, states.myDelta * multiplier);
    }

    void HandleLeftHandIKWeight()
    {
        //same as the right hand but with some extra cases
        float mutliplier = 3;

        if (states.inCover)
        {            
            if (!LHIK_dis_notAiming)
            {
                targetLHweight = 1;
                mutliplier = 6;
            }
            else//just add the cases for if we want to use the left hand for other things
            {
                mutliplier = 10;

                if(states.aiming)
                {
                    targetLHweight = 1;
                }
                else
                {
                    targetLHweight = 0;
                    leftHandIKweight = 0;
                }          
            }
        }
        else
        {
            if (!LHIK_dis_notAiming)
            {
                targetLHweight = 1;

                mutliplier = 10;
            }
            else
            {
                mutliplier = 10;
                targetLHweight = (states.aiming) ? 1 : 0;
            }
        }

        if (states.reloading || states.vaulting || disableIK)
        {
            targetLHweight = 0;
            mutliplier = 10;
        }

        if(disableIK)
        {
            leftHandIKweight = 0;
        }
        

        leftHandIKweight = Mathf.Lerp(leftHandIKweight, targetLHweight, states.myDelta * mutliplier);
    }

    void HandleShoulderRotation()//pretty much the same
    {
        aimHelperRS.position = Vector3.Lerp(aimHelperRS.position, states.lookPosition, states.myDelta * 5);
        weaponHolder.LookAt(aimHelperRS.position);

        if (rightHandIkTarget)
        {
            rightHandIkTarget.parent.transform.LookAt(aimHelperRS.position);
            rightHandIkTarget.transform.LookAt(aimHelperRS.position);
        }
    }

    void OnAnimatorIK()
    {

        if (leftHandIkTarget)
            leftHandHelper.position = leftHandIkTarget.position;

        anim.SetLookAtWeight(lookWeight, bodyWeight, headWeight, headWeight, clampWeight);

        Vector3 filterDirection = states.lookPosition;
        //filterDirection.y = offsetY; if needed
        anim.SetLookAtPosition(
            (overrideLookTarget != null)?
            overrideLookTarget.position:filterDirection
            );

        //Added else statements so if they don't have ik targets, we close the iks

        if(leftHandIkTarget)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKweight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHelper.position);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKweight);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIkTarget.rotation);    
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }

        if (rightHandIkTarget)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIkWeight);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandIkTarget.position);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIkWeight);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKRotation.rotation);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        }

        //added the same for elbows on both hands

        if(rightElbowTarget)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightHandIkWeight);
            anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowTarget.position);
        }
        else
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }

        if(leftElbowTarget)
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftHandIKweight);
            anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowTarget.position);
        }
        else
        {
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }

    }
}
