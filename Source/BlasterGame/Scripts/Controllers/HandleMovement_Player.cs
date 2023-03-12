using UnityEngine;
using System.Collections;

namespace TPC
{
    public class HandleMovement_Player : MonoBehaviour
    {
        StateManager states;
        Rigidbody rb;

        public bool doAngleCheck = true;
        [SerializeField]
        float degreesRunThreshold = 8;
        [SerializeField]
        bool useDot = true;

        bool overrideForce;
        bool inAngle;

        float rotateTimer_;
        float velocityChange = 4;
        bool applyJumpForce;

        float turnAngle;
        float movement;
        Vector3 storeDirection;
        InputHandler ih;

        Vector3 curVelocity;
        Vector3 targetVelocity;
        float prevAngle;
        Vector3 prevDir;

        bool overrideCanInterupted;
        bool interuptOverride;
        Vector3 overrideDirection;
        float overrideSpeed;
        float forceOverrideTimer;
        float forceOverLife;
        bool stopVelocity;
        bool useForceCurve;
        AnimationCurve forceCurve;
        float fc_t;
        bool initVault;
        Vector3 startPosition;

        bool forceOverHasRan;
        delegate void ForceOverrideStart();
        ForceOverrideStart forceOverStart;
        delegate void ForceOverrideWrap();
        ForceOverrideWrap forceOverWrap;

        BezierCurve climbCurve;
        bool enableRootMovement;
        
        
        public void Init(StateManager st, InputHandler inh)
        {
            ih = inh;
            states = st;
            rb = st.rBody;
            states.anim.applyRootMotion = false;

            GameObject curvePrefab = Resources.Load("CurveHolder_TPC") as GameObject;
            GameObject go = Instantiate(curvePrefab) as GameObject;
            climbCurve = go.GetComponentInChildren<BezierCurve>();
        }

        public void Tick()
        {
            if (states.curState == StateManager.CharStates.vaulting)
            {
                if (!initVault)
                {
                    VaultLogicInit();
                    initVault = true;
                }
                else
                {
                    HandleVaulting();
                }
                return;
            }

            if (!overrideForce && !initVault)
            {
                HandleDrag();   
                //if (states.onLocomotion)
                    MovementNormal();
                HandleJump();   
            }
            else
            {
                states.horizontal = 0;
                states.vertical = 0;
                states.anim.SetFloat(Statics.horizontal, states.horizontal);
                states.anim.SetFloat(Statics.vertical, states.vertical);
                OverrideLogic();
            }
        }

        void MovementNormal()
        {
            //for variable speed, not direction!
            float abs_v = Mathf.Abs(states.vertical);
            float abs_h = Mathf.Abs(states.horizontal);
            movement = Mathf.Abs(Mathf.Clamp01(abs_v + abs_h));

            //inAngle = states.inAngle_MoveDir;
            inAngle = true;

            //for direction
            Vector3 v = ih.camManager.transform.forward * states.vertical;
            Vector3 h = ih.camManager.transform.right * states.horizontal;

            v.y = 0;
            h.y = 0;     

            if (states.onGround)
            {
                HandleRotation();

                float targetSpeed = states.walk_f_speed;

                if (states.vertical < 0)
                    targetSpeed = states.walk_b_speed;
               
                if (states.run && states.groundAngle == 0
                    && states.anim.GetBool(Statics.onSprint))
                {
                    targetSpeed = states.sprintSpeed;
                }

                if (states.crouching)
                    targetSpeed = states.walk_c_speed;

                if (states.aiming)
                    targetSpeed = states.aimSpeed;

                if (inAngle)
                    HandleVelocity_Normal(h, v, targetSpeed);
                else
                    rb.velocity = Vector3.zero;
            }

            HandleAnimations_Normal();

        }

        void HandleVelocity_Normal(Vector3 h, Vector3 v, float speed)
        {
            Vector3 curVelocity = rb.velocity;

            if (states.curState == StateManager.CharStates.moving)
            {
                targetVelocity = (h + v).normalized * (speed * movement);
                velocityChange = 3;
            }
            else
            {
                velocityChange = 2;
                targetVelocity = Vector3.zero;
            }

            Vector3 vel = Vector3.Lerp(curVelocity, targetVelocity, Time.deltaTime * velocityChange);
            rb.velocity = vel;

            if (states.obstacleForward)
                rb.velocity = Vector3.zero;
        }

        void HandleRotation_Normal(Vector3 h, Vector3 v)
        {
            if (states.curState == StateManager.CharStates.moving)
            {
                storeDirection = (v + h).normalized;

                float targetAngle = Mathf.Atan2(storeDirection.x, storeDirection.z) * Mathf.Rad2Deg;

                if (states.run && doAngleCheck)
                {
                    if (!useDot)
                    {
                        if ((Mathf.Abs(prevAngle - targetAngle)) > degreesRunThreshold)
                        {
                            prevAngle = targetAngle;
                            PlayAnimSpecial(AnimSpecials.runToStop, false);
                            return;
                        }
                    }
                    else
                    {
                        float dot = Vector3.Dot(prevDir, states.moveDirection);
                        if (dot < 0)
                        {
                            prevDir = states.moveDirection;
                            PlayAnimSpecial(AnimSpecials.runToStop, false);
                            return;
                        }
                    }
                }

                prevDir = states.moveDirection;
                prevAngle = targetAngle;

                storeDirection += transform.position;
                Vector3 targetDir = (storeDirection - transform.position).normalized;
                targetDir.y = 0;
                if (targetDir == Vector3.zero)
                    targetDir = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, velocityChange * Time.deltaTime);
            }
        }

        void HandleRotation()
        {
            float speed =2;

            if (states.curState == StateManager.CharStates.moving)
                speed = 3;

            Ray ray = new Ray(ih.camManager.camTrans.position, ih.camManager.camTrans.forward);
            Vector3 forwardPos = ray.GetPoint(50);

            Vector3 targetDir = (forwardPos - transform.position).normalized;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            turnAngle = Vector3.Angle(transform.forward, targetDir);

            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, speed * Time.deltaTime);
        }

        void HandleAnimations_Normal()
        {
            float h = states.horizontal;
            float v = states.vertical;

            if (states.obstacleForward)
                v = 0;

            if(states.aiming || states.crouching)
            {
                h = Mathf.Clamp(h, -0.5f, 0.5f);
                v = Mathf.Clamp(v, -0.5f, 0.5f);
            }

            float turn = turnAngle / 45;

            states.anim.SetBool(Statics.crouch_anim, states.crouching);
            states.anim.SetFloat(Statics.vertical, v, 0.2f, Time.deltaTime);
            states.anim.SetFloat(Statics.horizontal, h, 0.2f, Time.deltaTime);
            states.anim.SetFloat(Statics.turn, turn, 0.2f, Time.deltaTime);
        }

        void HandleJump()
        {
            if (states.onGround && states.canJump_b)
            {
                if (states.jumpInput && !states.jumping && states.onLocomotion
                    && states.curState != StateManager.CharStates.hold && states.curState != StateManager.CharStates.onAir)
                {
                    if (states.curState == StateManager.CharStates.idle || states.obstacleForward)
                    {
                        states.anim.SetBool(Statics.special, true);
                        states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialType(AnimSpecials.jump_idle));
                    }

                    if (states.curState == StateManager.CharStates.moving && !states.obstacleForward)
                    {
                        states.LegFront();
                        states.jumping = true;
                        states.anim.SetBool(Statics.special, true);
                        states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialType(AnimSpecials.run_jump));
                        states.curState = StateManager.CharStates.hold;
                        states.anim.SetBool(Statics.onAir, true);
                        states.canJump_b = false;
                    }
                }
            }

            if (states.jumping)
            {
                if (states.onGround)
                {
                    if (!applyJumpForce)
                    {
                        StartCoroutine(AddJumpForce(0));
                        applyJumpForce = true;
                    }
                }
                else
                {
                    states.jumping = false;
                }
            }
            else
            {

            }
        }

        IEnumerator AddJumpForce(float delay)
        {
            yield return new WaitForSeconds(delay);
            rb.drag = 0;
            Vector3 vel = rb.velocity;
            Vector3 forward = transform.forward;
            vel = forward * 3;
            vel.y = states.jumpForce;
            rb.velocity = vel;
            StartCoroutine(CloseJump());
        }

        IEnumerator CloseJump()
        {
            yield return new WaitForSeconds(0.3f);
            states.curState = StateManager.CharStates.onAir;
            states.jumping = false;
            applyJumpForce = false;
            states.canJump_b = false;
            StartCoroutine(EnableJump());
        }

        IEnumerator EnableJump()
        {
            yield return new WaitForSeconds(1.3f);
            states.canJump_b = true;
        }

        void HandleDrag()
        {
            if (states.curState == StateManager.CharStates.moving || states.onGround == false)
            {
                rb.drag = 0;
            }
            else
            {
                rb.drag = 4;
            }
        }

        public void PlayAnimSpecial(AnimSpecials t, bool sptrue = true)
        {
            int n = Statics.GetAnimSpecialType(t);
            states.anim.SetBool(Statics.special, sptrue);
            states.anim.SetInteger(Statics.specialType, n);
            StartCoroutine(CloseSpecialOnAnim(0.4f));
        }

        IEnumerator CloseSpecialOnAnim(float t)
        {
            yield return new WaitForSeconds(t);
            states.anim.SetBool(Statics.special, false);
        }

        bool canVault;//different from the states.canvault
        Vector3 targetVaultPosition;

        void VaultLogicInit()
        {
            canVault = states.canVault_b;
            states.canVault_b = false;
            VaultPhaseInit(states.targetVaultPosition);
        }

        public bool vaultFromCover;

        void VaultPhaseInit(Vector3 targetPos)
        {
            states.controllerCollider.isTrigger = true;

            switch (states.curVaultType)
            {
                case StateManager.VaultType.idle:
                case StateManager.VaultType.walk:
                    overrideSpeed = Statics.vaultSpeedWalking; //How fast we basically vault
                    states.anim.CrossFade(Statics.walkVault, 0.1f);
                    break;
                case StateManager.VaultType.run:
                    overrideSpeed = Statics.vaultSpeedRunning; //How fast we basically vault
                    states.anim.CrossFade(Statics.runVault, 0.05f);
                    break;
                case StateManager.VaultType.walk_up:           
                    overrideSpeed = Statics.walkUpSpeed; 
                    if (!states.run)
                    { states.anim.CrossFade(Statics.walk_up, 0.05f); }
                    else
                    {
                        states.anim.CrossFade(Statics.run_up, 0.1f);
                        overrideSpeed = Statics.vaultSpeedRunning;
                    }
                    break;
                case StateManager.VaultType.climb_up:
                    states.anim.CrossFade(Statics.climb_up, 0.4f);
                    overrideSpeed = Statics.climbSpeed;
                    break;
            }

            //let's reuse the same variables as for force override
            forceOverrideTimer = 0;
            //Since we're going to do a lerp, we don't need to use force over life for a timer
            //we know the lerp is over after the t has reached 1+
            //so we're gonna use the float as a container for the distance
            forceOverLife = Vector3.Distance(transform.position, targetPos);
            fc_t = 0;
          
            states.rBody.isKinematic = true;
            startPosition = (vaultFromCover) ? transform.position + ih.coverNormal * 0.5f
              : transform.position;
            overrideDirection = targetPos - startPosition;
            overrideDirection.y = 0;         
            targetVaultPosition = targetPos;

            if(states.curVaultType == StateManager.VaultType.climb_up)
            {
                startPosition = transform.position;
                targetVaultPosition = states.startVaultPosition;
                overrideDirection = targetPos - startPosition;
                overrideDirection.y = 0;
            }

            vaultFromCover = false;
        }

        public void HandleVaulting()
        {
            if (states.curVaultType == StateManager.VaultType.climb_up)
            {
                HandleCurveMovement();
                return;
            }

            //this will ensure the curve is sampled on the actual length of the lerp
            //fc_t += Time.deltaTime;
            float targetSpeed = overrideSpeed; //* ih.vaultCurve.Evaluate(fc_t);

            forceOverrideTimer += Time.deltaTime * targetSpeed / forceOverLife;

            if(forceOverrideTimer > 1)
            {
                forceOverrideTimer = 1;
                StopVaulting();
            }

            Vector3 targetPosition = Vector3.Lerp(startPosition,
               targetVaultPosition, forceOverrideTimer);
            transform.position = targetPosition;

            //HandleRotation
            if (overrideDirection == Vector3.zero)
                overrideDirection = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(overrideDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
        }

        void StopVaulting()
        {
            states.curState = StateManager.CharStates.moving;
            states.vaulting = false;
            states.controllerCollider.isTrigger = false;
            states.rBody.isKinematic = false;
            states.skipGroundCheck = false;
            initVault = false;
            StartCoroutine("OpenCanVaultIfApplicable");
            isAtStart = false;
        }

        IEnumerator OpenCanVaultIfApplicable()
        {
            yield return new WaitForSeconds(0.4f);
            states.canVault_b = canVault;//enable it if the user has enabled it
        }

        public void AddVelocity(Vector3 direction,float t,float force, bool clamp, bool useFCurve, AnimationCurve fcurve, bool canInterupt)
        {
            if (states.vaulting)
                return;

            overrideCanInterupted = canInterupt;
            forceOverLife = t;
            overrideSpeed = force;
            overrideForce = true;
            forceOverrideTimer = 0;
            overrideDirection = direction;
            rb.velocity = Vector3.zero;
            stopVelocity = clamp;
            forceCurve = fcurve;
            useForceCurve = useFCurve;
            fc_t = 0;
        }

        void OverrideLogic()
        {
            rb.drag = 0;

            if (!forceOverHasRan)//Run any delegates we have assigned on start
            {
                if (forceOverStart != null)
                    forceOverStart();

                forceOverHasRan = true;
            }

            float targetSpeed = overrideSpeed;

            if (useForceCurve)
            {
                fc_t += Time.deltaTime / forceOverLife;
                targetSpeed *= forceCurve.Evaluate(fc_t);
            }

            rb.velocity = overrideDirection * targetSpeed;

            if (!states.onGround)
                interuptOverride = true;

            forceOverrideTimer += Time.deltaTime;
            if(forceOverrideTimer > forceOverLife || overrideCanInterupted && interuptOverride)
            {
                if(stopVelocity)
                    rb.velocity = Vector3.zero;

                stopVelocity = false;
                overrideForce = false;
                forceOverHasRan = false;
                interuptOverride = false;

                if (forceOverWrap != null)//Run any delegates we have assigned on end
                    forceOverWrap();

                forceOverWrap = null;
                forceOverStart = null;
            }
        }

        public void EnableRootMovement()
        {
            enableRootMovement = true;
        }

        void InitClimbCurve()
        {
            startPosition = states.startVaultPosition;
            targetVaultPosition = states.targetVaultPosition;
            overrideDirection = targetVaultPosition - startPosition;
            overrideDirection.y = 0;
            enableRootMovement = false;
            climbCurve.transform.position = startPosition;
            climbCurve.transform.rotation = Quaternion.LookRotation(overrideDirection);
            BezierPoint[] points = climbCurve.GetAnchorPoints();
            points[0].transform.position = startPosition;
            points[points.Length - 1].transform.position = 
                targetVaultPosition + Vector3.up *0.05f;
            forceOverrideTimer = 0;
        }

        bool isAtStart;

        void HandleCurveMovement()
        {
            if (!isAtStart)
            {
                forceOverrideTimer += Time.deltaTime * 5;
                if (forceOverrideTimer > 1)
                {
                    forceOverrideTimer = 1;
                    InitClimbCurve();
                    isAtStart = true;
                }

                Vector3 targetPos = Vector3.Lerp(startPosition, targetVaultPosition, forceOverrideTimer);
                transform.position = targetPos;
            }
            else
            {
                if (enableRootMovement)
                {
                    forceOverrideTimer += Time.deltaTime * overrideSpeed;
                }

                if (forceOverrideTimer > 0.95f)
                {
                    forceOverrideTimer = 1;
                    StopVaulting();
                }

                Vector3 targetPos = climbCurve.GetPointAt(forceOverrideTimer);
                transform.position = targetPos;

                 if (overrideDirection == Vector3.zero)
                     overrideDirection = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation(overrideDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
            }
        }
    }
}
