using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Weapons;

namespace TPC
{
    public class StateManager : MonoBehaviour
    {
        [Header("Stats")]
        public int health = 100;
        public bool isDead;

        [Header("Info")]
        public GameObject modelPrefab;
        public string modelRig;
        public bool inGame;
        public bool isPlayer;

        [Header("State variables")]
        public float groundDistance = 0.6f;
        public float groundOffset = 0;
        public float distanceToCheckForward = 1.3f;
        public float sprintSpeed = 6;
        public float walk_f_speed = 4;
        public float walk_b_speed = 3;
        public float walk_c_speed = 2;
        public float aimSpeed = 2;
        public float jumpForce = 4;
        public float airTimeThreshold = 0.78f;
        public float vaultOverHeight = 1.5f;
        public float vaultFloorHeightDiffernce = 0.3f;
        public int coverDirection = 1;

        [Header("Inputs")]
        public float horizontal;
        public float vertical;
        public bool jumpInput;

        [Header("States")]
        public bool mainShoulderIsLeft;
        public bool canAim;
        public bool aiming;     
        public bool actualShooting;
        public bool obstacleForward;
        public bool groundForward;
        public float groundAngle;
        public bool canVault;
        public bool vaulting;
        public bool jumping;
        public bool reloading;
        public bool inAction;
        public bool switchingWeapon;
        public bool hold;
        public bool canCover;
        public bool inCover;
        public bool crouching;
        public bool inCoverCanAim;

        #region StateRequests
        [Header("State Requests")]
        public bool shooting;
        public CharStates curState;
        public bool onGround;
        public bool run;
        public bool walk;
        public bool onLocomotion;
        public bool inAngle_MoveDir;
        public bool inAngle_Aim;       
        public bool switchWeapon;
        public bool reload;
        #endregion

        #region Enable Disable Features
        public bool canRun_b = true;
        public bool canJump_b;
        public bool canVault_b = true;
        #endregion

        #region References
        [HideInInspector]
        public WeaponManager weaponManager;
        [HideInInspector]
        public BoneHelpers bHelpers;
        [HideInInspector]
        public IKHandler ikHandler;
        [HideInInspector]
        public GameObject activeModel;
        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public Rigidbody rBody;
        [HideInInspector]
        public Collider controllerCollider;
        #endregion

        #region Variables
        [HideInInspector]
        public Vector3 moveDirection;
        [HideInInspector]
        public Vector3 aimPosition;
        public float airTime;
        [HideInInspector]
        public bool prevGround;
        [HideInInspector]
        public Vector3 targetVaultPosition;
        [HideInInspector]
        public Vector3 startVaultPosition;
        [HideInInspector]
        public bool skipGroundCheck;
     //   [HideInInspector]
       

        public enum VaultType
        {
            idle, walk, run, walk_up, climb_up
        }

        [HideInInspector]
        public VaultType curVaultType;
        #endregion

        [HideInInspector]
        public LayerMask ignoreLayers;

        public enum CharStates
        {
            idle, moving, onAir, hold, vaulting, cover
        }

        bool createAudio = false;
        AudioSource audioSource;

        bool hasRagdoll;
        List<Rigidbody> ragdollRigidBodies = new List<Rigidbody>();
        List<Collider> ragdollColliders = new List<Collider>();

        #region Init Phase
        public void Init()
        {
            inGame = true;
            CreateModel();
            SetupAnimator();
            AddControllerReferences();
            canJump_b = true;

            gameObject.layer = 8;
            ignoreLayers = ~(1 << 2 | 1 << 8 | 1 << 9);

            controllerCollider = GetComponent<Collider>();
            if (controllerCollider == null)
            {
                Debug.Log("No collider found for the controller!!");
            }

            bHelpers.Init(anim);

            if(createAudio)
            {
                gameObject.AddComponent<AudioSource>();
                audioSource = GetComponent<AudioSource>();
            }

            InitRagdoll();
        }

        void CreateModel()
        {
            activeModel = Instantiate(modelPrefab) as GameObject;
            activeModel.transform.parent = this.transform;
            activeModel.transform.localPosition = Vector3.zero;
            activeModel.transform.localEulerAngles = Vector3.zero;
            activeModel.transform.localScale = Vector3.one;
        }

        void SetupAnimator()
        {
            anim = GetComponent<Animator>();
            Animator childAnim = activeModel.GetComponent<Animator>();
            anim.avatar = childAnim.avatar;
            Destroy(childAnim);
        }

        void AddControllerReferences()
        {
            gameObject.AddComponent<Rigidbody>();
            rBody = GetComponent<Rigidbody>();
            rBody.angularDrag = 999;
            rBody.drag = 4;
            rBody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;

            gameObject.AddComponent<BoneHelpers>();
            bHelpers = GetComponent<BoneHelpers>();
           
            ikHandler = GetComponent<IKHandler>();
            ikHandler.Init(this);

            weaponManager = GetComponent<WeaponManager>();     
        }
        #endregion

        void MonitorInAction()
        {
            inAction = false;
            //if (reloading)
             //   inAction = true;
            if (switchingWeapon)
                inAction = true;
            if (vaulting)
                inAction = true;

            bool inIdle = anim.GetBool(Statics.inIdle);

            if(inIdle && !hold)
            {
                reloading = false;
                switchingWeapon = false;
                anim.SetBool(Statics.closeIK, false);
            }
        }

        public void FixedTick()
        {
            //states that don't run the update in the state manager
            if (curState == CharStates.hold
                || curState == CharStates.vaulting || curState == CharStates.cover)
                return;

            obstacleForward = false;
            groundForward = false;
            onGround = OnGround();

            if (onGround)
            {
                Vector3 origin = transform.position;
                //Clear forward
                origin += Vector3.up * 0.75f;
                IsClear(origin, transform.forward, distanceToCheckForward, ref obstacleForward);

                if (!obstacleForward && !vaulting)
                {
                    //is ground forward?
                    origin += transform.forward * 0.6f;
                    IsClear(origin, -Vector3.up, groundDistance * 3, ref groundForward);
                }
                else
                {
                    if (Vector3.Angle(transform.forward, moveDirection) > 30)
                    {
                        obstacleForward = false;
                    }
                }
            }

            UpdateState();
            MonitorAirTime();
        }

        public void RegularTick()
        {
            onGround = OnGround();
            MonitorInAction();

            if (reload)
                weaponManager.ReloadWeapon();

            if (switchWeapon)
                weaponManager.ChangeWeapon();

            if (!inAction)
                weaponManager.Tick();
        }

        public void LateTick()
        {
            onGround = OnGround();  
        }

        void UpdateState()
        {
            if (curState == CharStates.hold)
                return;

            if (vaulting)
            {
                curState = CharStates.vaulting;
                crouching = false;
                inCover = false;
                return;
            }

            if (horizontal != 0 || vertical != 0)
            {
                curState = CharStates.moving;
            }
            else
            {
                curState = CharStates.idle;
            }

            if (!onGround)
            {
                curState = CharStates.onAir;
            }
        }

        public bool OnGround()
        {
            if (skipGroundCheck)
                return false;

            bool r = false;

            if (curState == CharStates.hold)
                return false;

            Vector3 origin = transform.position + (Vector3.up * 0.55f);

            RaycastHit hit = new RaycastHit();
            bool isHit = false;
            FindGround(origin, ref hit, ref isHit);

            if (!isHit)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 newOrigin = origin;

                    switch (i)
                    {
                        case 0://forward
                            newOrigin += Vector3.forward / 3;
                            break;
                        case 1://backwards
                            newOrigin -= Vector3.forward / 3;
                            break;
                        case 2://left
                            newOrigin -= Vector3.right / 3;
                            break;
                        case 3://right
                            newOrigin += Vector3.right / 3;
                            break;
                    }

                    FindGround(newOrigin, ref hit, ref isHit);

                    if (isHit == true)
                        break;
                }
            }

            r = isHit;

            if (r != false)
            {
                Vector3 targetPosition = transform.position;
                targetPosition.y = hit.point.y + groundOffset;
                transform.position = targetPosition;
            }

            return r;
        }

        void FindGround(Vector3 origin, ref RaycastHit hit, ref bool isHit)
        {
         //   Debug.DrawRay(origin, -Vector3.up * 0.5f, Color.red);
            if (Physics.Raycast(origin, -Vector3.up, out hit, groundDistance, ignoreLayers))
            {
                isHit = true;
            }
        }

        void IsClear(Vector3 origin, Vector3 direction, float distance, ref bool isHit)
        {
            RaycastHit hit = new RaycastHit();
            float targetDistance = distance;
          //  if (run)
            //    targetDistance += 0.5f;

            int numberOfHits = 0;
            for (int i = -1; i < 2; i++)
            {
                Vector3 targetOrigin = origin;
                targetOrigin += transform.right * (i * 0.3f);
               // Debug.DrawRay(targetOrigin, direction * targetDistance, Color.green);
                if (Physics.Raycast(targetOrigin, direction, out hit, targetDistance, ignoreLayers))
                {
                    numberOfHits++;
                }
            }

            if (numberOfHits > 2)
            {
                isHit = true;
            }
            else
            {
                isHit = false;
            }

            if (aiming)
                return;

            if (obstacleForward)
            {
                Vector3 incomingVec = hit.point - origin;
                Vector3 reflectVect = Vector3.Reflect(incomingVec, hit.normal);
                float angle = Vector3.Angle(incomingVec, reflectVect);

                if (angle < 70)
                {
                    obstacleForward = false;
                }
                else
                {
                    //Moved to Controller_Extras
                }
            }

            if (groundForward)
            {
                if (curState == CharStates.moving)
                {
                    Vector3 p1 = transform.position;
                    Vector3 p2 = hit.point;
                    float diffY = p1.y - p2.y;
                    groundAngle = diffY;
                }

                float targetIncline = 0;

                if (Mathf.Abs(groundAngle) > 0.2f)
                {
                    if (groundAngle < 0)
                        targetIncline = 1;
                    else
                        targetIncline = -1;
                }
                else
                {
                    groundAngle = 0;
                }

                if (groundAngle == 0)
                    targetIncline = 0;

                anim.SetFloat(Statics.incline, targetIncline, 0.3f, Time.deltaTime);
            }
        }

        void ClimbOver(RaycastHit hit, ref bool willClimb, ClimbCheckType ct)
        {
            float targetDistance = distanceToCheckForward + 0.1f;
            if (run)
                targetDistance += 0.5f;

            Vector3 climbCheckOrigin = transform.position;
            switch (ct)
            {
                case ClimbCheckType.walk_up:
                    climbCheckOrigin += Vector3.up * Statics.walkUpHeight;
                    break;
                case ClimbCheckType.climb_up:
                    climbCheckOrigin += Vector3.up * Statics.climbMaxHeight;
                    break;
            }
            RaycastHit climbHit;

            Vector3 wallDirection = -hit.normal * targetDistance;
           // Debug.DrawRay(climbCheckOrigin, wallDirection, Color.yellow);
            if (Physics.Raycast(climbCheckOrigin, wallDirection, out climbHit, 1.2f, ignoreLayers))
            {

            }
            else
            {
                Vector3 origin2 = hit.point;
                origin2.y = transform.position.y;
                switch (ct)
                {
                    case ClimbCheckType.walk_up:
                        origin2 += Vector3.up * Statics.walkUpHeight;
                        break;
                    case ClimbCheckType.climb_up:
                        origin2 += Vector3.up * Statics.climbMaxHeight;
                        break;
                }
               
                origin2 += wallDirection * 0.2f;
              //  Debug.DrawRay(origin2, -Vector3.up, Color.yellow);

                if (Physics.Raycast(origin2, -Vector3.up, out climbHit, 1, ignoreLayers))
                {
                    float diff = climbHit.point.y - transform.position.y;
                    if (Mathf.Abs(diff) > Statics.walkUpThreshold)
                    {
                        vaulting = true;
                        targetVaultPosition = climbHit.point;                 
                        obstacleForward = false;
                        willClimb = true;

                        switch (ct)
                        {
                            case ClimbCheckType.walk_up:
                                curVaultType = VaultType.walk_up;
                                skipGroundCheck = true;
                                break;
                            case ClimbCheckType.climb_up:
                                curVaultType = VaultType.climb_up;
                                skipGroundCheck = true;
                                Vector3 startPos = hit.normal * Statics.climbUpStartPosOffset;                       
                                startPos = hit.point + startPos;
                                startPos.y = transform.position.y;
                                startVaultPosition = startPos;

                                float climbDiff = targetVaultPosition.y - transform.position.y;

                             /*   if (Mathf.Abs(climbDiff) > 1.7f)
                                {
                                    climbAnimName = Statics.climb_up;
                                }
                                else
                                {
                                    climbAnimName = Statics.climb_up;
                                }*/
                                break;
                        }

                        return;
                    }
                }
            }
        }

        enum ClimbCheckType
        {
            walk_up,climb_up
        }

        void MonitorAirTime()
        {
            if(!jumping)
                anim.SetBool(Statics.onAir, !onGround);

            if (onGround)
            {
                if (prevGround != onGround)
                {
                    anim.SetInteger(Statics.jumpType,
                        (airTime > airTimeThreshold) ?
                        (horizontal != 0 || vertical != 0) ? 2 : 1
                        : 0);
                }

                airTime = 0;
            }
            else
            {
                airTime += Time.deltaTime;
            }

            prevGround = onGround;
        }

        public void LegFront()
        {
            Vector3 ll = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Vector3 rl = anim.GetBoneTransform(HumanBodyBones.RightFoot).position;
            Vector3 rel_ll = transform.InverseTransformPoint(ll);
            Vector3 rel_rr = transform.InverseTransformPoint(rl);

            bool left = rel_ll.z > rel_rr.z;
            anim.SetBool(Statics.mirrorJump, left);           
        }

        void InitRagdoll()
        {
            Rigidbody[] rigids = GetComponentsInChildren<Rigidbody>();
            Collider[] cols = GetComponentsInChildren<Collider>();

            foreach (Rigidbody r in rigids)
            {
                if(r != rBody)
                {
                    ragdollRigidBodies.Add(r);
                    r.isKinematic = true;
                    r.gameObject.layer = 9;
                }
            }

            foreach (Collider c in cols)
            {
                if(c != controllerCollider)
                {
                    ragdollColliders.Add(c);
                    c.isTrigger = true;
                }
            }

            hasRagdoll = (ragdollRigidBodies.Count > 2);
        }

        public void SubtractHealth(int value)
        {
            health -= value;

            if(health <= 0)
            {
                KillCharacter();
            }
        }

        void KillCharacter()
        {
            if (isDead)
                return;

            isDead = true;

            anim.CrossFade(Statics.death, 0.3f);
            MonoBehaviour[] monos = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour m in monos)
            {
                m.enabled = false;
            }

            if (hasRagdoll)
            {
                foreach (Rigidbody r in ragdollRigidBodies)
                {
                    r.isKinematic = false;
                    r.gameObject.layer = 9;
                }

                foreach (Collider col in ragdollColliders)
                {
                    col.isTrigger = false;
                }
            }
            else
            {
                CapsuleCollider cap = GetComponent<CapsuleCollider>();
                Vector3 cen = Vector3.up * 0.5f;
                cap.center = cen;
            }

            anim.enabled = true;

            StartCoroutine("DestroyCharacter");

        }

        IEnumerator DestroyCharacter()
        {
            yield return new WaitForSeconds(3);
            activeModel.transform.parent = null;
            Destroy(gameObject);
        }
    }
    
}
