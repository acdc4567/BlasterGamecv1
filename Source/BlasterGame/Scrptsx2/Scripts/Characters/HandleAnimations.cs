using UnityEngine;
using System.Collections;

public class HandleAnimations : MonoBehaviour {

    public Animator anim;//made it public

    StateManager states;
    Vector3 lookDirection;

    public void Init()
    {
        states = GetComponent<StateManager>();    
        SetupAnimator();
    }
	
	public void  Tick() {

        anim.SetBool("OnGround", (!states.vaulting) ? states.onGround : true);

        if (!states.canRun)
        {
            anim.SetFloat("Forward", states.vertical, 0.1f, states.myDelta);
            anim.SetFloat("Sideways", states.horizontal, 0.1f, states.myDelta);
        }
        else
        {
            float movement = Mathf.Abs(states.vertical) + Mathf.Abs(states.horizontal);

            bool walk = states.walk;

            movement = Mathf.Clamp(movement, 0, (walk || states.reloading || states.crouching) ? 0.5f : 1);

            anim.SetFloat("Forward", movement, 0.1f, states.myDelta);
        }

        Tick_Common();
    }

    public void Tick_Common()
    {
        anim.speed = 1 * states.tm.myTimeScale;

        states.reloading = anim.GetBool("Reloading");
        anim.SetBool("Aim", states.aiming);
        anim.SetInteger("WeaponType", states.weaponAnimType);
        anim.SetFloat("Stance", states.stance);
        anim.SetBool("AimAtSides", states.aimAtSides);

        if (states.aiming || states.inCover || states.down)
        {
            anim.SetBool("ExitLocomotion", true);
        }
        else
        {
            anim.SetBool("ExitLocomotion", false);
        }
    }

    public void SetupAnimator(Animator targetAnim = null)
    {
        anim = GetComponent<Animator>();

        if (targetAnim == null)
        {
            Animator[] anims = GetComponentsInChildren<Animator>();

            for (int i = 0; i < anims.Length; i++)
            {
                if (anims[i] != anim)
                {
                    anim.avatar = anims[i].avatar;
                    Destroy(anims[i]);
                    break;
                }
            }
        }
        else
        {
            anim.avatar = targetAnim.avatar;
            Destroy(targetAnim);
        }
    }

    public void StartReload()
    {
        if(!states.reloading)
        {
            anim.SetTrigger("Reload");
        }
    }
}
