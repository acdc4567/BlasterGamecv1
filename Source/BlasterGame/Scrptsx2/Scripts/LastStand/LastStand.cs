using UnityEngine;
using System.Collections;

public class LastStand : MonoBehaviour {

    InputHandler ih;
    [HideInInspector]
    public Rigidbody rb;
    StateManager states;
    Collider col;
    bool initDownState;
    PhysicMaterial zFriction;
    PhysicMaterial mFriction;

    float horizontal;
    float vertical;

    float moveDuration = 1;
    float crawlSpeed = 0.1f;

    bool move;
    bool faceDown;
    float curTime;
    Vector3 moveDirection;

    public WeaponReferenceBase downWeapon;

    public void  Init(StateManager st) {

        ih = GetComponent<InputHandler>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        states = st;

        zFriction = new PhysicMaterial();
        zFriction.dynamicFriction = 0;
        zFriction.staticFriction = 0;

        mFriction = new PhysicMaterial();
        mFriction.dynamicFriction = 1;
        mFriction.staticFriction = 1;
    }

    public void Tick() {

        if(!initDownState)
        {
            if (states.weaponManager.ReturnCurrentWeapon() != downWeapon)
            {
                states.handleAnim.anim.CrossFade("Hit_to_Down", 0.3f);
                states.handleAnim.anim.SetBool("Down", true);
                states.weaponManager.SwitchWeaponWithTargetWeapon(downWeapon);
                states.dummyModel = true;
            }

            initDownState = true;
        }

        Vector3 directionToLP = (states.lookPosition - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToLP);
        faceDown = (angle > 100);
        states.handleAnim.anim.SetBool("FaceDown", faceDown);
        states.handleAnim.anim.SetBool("Aim", states.aiming);
        HandleAnim(states.vertical, states.horizontal);

        if (move || faceDown && states.aiming)
        {
            Vector3 targetDir = directionToLP;
            targetDir.y = 0;
            Quaternion targetRot =
                Quaternion.LookRotation(
                    (faceDown) ? -targetDir : targetDir
                    );

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, states.myDelta * 0.5f);
        }

        if (move && !states.aiming)
        {
            curTime += states.myDelta;

            if (curTime < moveDuration)
            {
                col.material = zFriction;
                rb.AddForce(moveDirection * crawlSpeed, ForceMode.VelocityChange);
            }
            else
            {
                crawlSpeed = 0.1f;
                moveDuration = 1;
                col.material = mFriction;
                curTime = 0;
                move = false;
            }
        }
        else
        {
            FindAngles();

            horizontal = states.horizontal;
            vertical = states.vertical;

            Vector3 v = ih.camHolder.forward * vertical;
            Vector3 h = ih.camHolder.right * horizontal;

            if(v != Vector3.zero || h != Vector3.zero)
            {
                move = true;
                moveDirection = (v + h).normalized;
            }
        }
    }

    void HandleAnim(float v, float h)
    {
        states.handleAnim.anim.SetFloat("Forward", v);
        states.handleAnim.anim.SetFloat("Sideways", h);
    }

    void FindAngles()
    {
        Vector3 dir = states.lookPosition - transform.position; 
        Vector3 relativePosition = transform.InverseTransformDirection(dir.normalized);

        float s = relativePosition.x;

        states.handleAnim.anim.SetFloat("AimSides", s, 0.5f, states.myDelta);
    }

    public void AddMovement(Vector3 relativeDirection, float duration,float speed)
    {
        Vector3 worldDir = transform.TransformDirection(relativeDirection);
        moveDirection = worldDir;
        move = true;
        moveDuration = duration;
        crawlSpeed = speed;
    }
}
