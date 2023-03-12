using UnityEngine;
using System.Collections;

namespace TPC
{
    public class Controller_Extras : MonoBehaviour
    {
        StateManager states;       
        bool movePositive;
        bool twoPointValidation = true;
        int coverDirection;
        float coverMovementSpeed = 1.5f;
        float minCover = 0.2f;
        float aheadDistance =3;
        float offsetFromWall = 0.6f;
        float sampleDis = 0.25f;

        bool canAim;
        bool _initLerp;
        bool initCover;
        float _length;
        float _lpSpeed = 2;
        Vector3 _targetPos;
        Vector3 _startPos;
        float _t;

        Vector3 relativeInput;
        CoverPosition cp;      
        Transform helper;

        UI.WorldCanvas c;
        InputHandler input_h;

        public void Init(StateManager st, InputHandler ih)
        {
            input_h = ih;
            states = st;
            cp = new CoverPosition();
            helper = new GameObject().transform;
            helper.name = "cover helper";
           
            c = UI.WorldCanvas.singleton;
        }

        public void Tick(Vector3 pivotPosition)
        {
            if (c.coverText.gameObject.activeInHierarchy)
            {
                c.coverText.gameObject.SetActive(false);
                c.vaultText.gameObject.SetActive(false);
            }

            states.canVault = false;

            if (states.vaulting || states.inAction)
                return;

            if (states.inCover)
            {
                if (!initCover)
                {
                    GetInCoverLerp();
                }
                else
                {
                    CoverMovement();
                }
            }
            else
            {
                LookForCover(transform.position);
            }

            if (states.canVault)
            {
                c.vaultText.gameObject.SetActive(true);
                c.vaultText.position = c.coverText.position + Vector3.up * 0.3f;
                c.vaultText.rotation = c.coverText.rotation;
            }
        }

        public void LookForCover(Vector3 pivotPosition)
        {
            states.canCover = false;
            //   if (states.inAction)
            //     return;

            pivotPosition.y = transform.position.y;
            // Vector3 startPos = ((pivotPosition - transform.position).normalized* 0)
            //   + transform.position;
            Vector3 startPos = pivotPosition;

            //Vector3 targetDir = states.aimPosition - startPos;
            Vector3 targetDir = transform.forward;
            targetDir.y = 0;
            RaycastHit hit;
            //how high up the raycast starts, essentially how high an object needs to be to be taken as a cover
            Vector3 origin = startPos + Vector3.up * 0.9f;
            Debug.DrawRay(origin, targetDir);
            if (Physics.Raycast(origin, targetDir, out hit, Statics.lookForCoverDistance, states.ignoreLayers))
            {
               if (hit.transform.GetComponent<BoxCollider>())
                {
                    Vector3 dir2 = hit.transform.position - transform.position;

                    float dot = Vector3.Dot(transform.forward, dir2);

                    if (dot > 0)
                    {
                        helper.transform.position = PosWithOffset(origin, hit.point);
                        helper.transform.rotation = Quaternion.LookRotation(-hit.normal);

                        bool right = isCoverValid(helper, true);
                        bool left = isCoverValid(helper, false);

                        //the cover is the minimum size
                        if (right || left)
                        {
                            states.canCover = true;
                            cp.initialHit = hit.point;
                            c.coverText.gameObject.SetActive(true);
                            Vector3 dir = hit.point - startPos;
                            c.coverText.position = startPos + dir * 0.95f;
                            c.coverText.LookAt(hit.point + (-hit.normal * 5));  
                            CanVaultOver(hit);
                            if (!states.canVault)
                                ClimbOver(hit);
                        }
                    }
                }
            }
        }

        void CanVaultOver(RaycastHit hit)
        {
            //We hit a wall around knee high
            //then we need to see if we can vault over it
            Vector3 wallDirection = -hit.normal * 0.5f;
            //the opossite of the normal, is going to return us the direction
            //if the whole level is set with box colliders, then this will work like a charm
            RaycastHit vHit;

            Vector3 wallOrigin = hit.point + (hit.normal * 0.1f);
            wallOrigin.y = transform.position.y;
            wallOrigin += Vector3.up * states.vaultOverHeight;
            Debug.DrawRay(wallOrigin, wallDirection * Statics.vaultCheckDistance, Color.red);

            if (Physics.Raycast(wallOrigin, wallDirection, out vHit, Statics.vaultCheckDistance, states.ignoreLayers))
            {
                //it's a wall
                //willVault = false;
                return;
            }
            else
            {
                //It's not a wall, but can we vault over it?
                if (states.canVault_b && !states.vaulting)
                {
                    Vector3 startOrigin = hit.point;
                    startOrigin.y = transform.position.y;
                    Vector3 vOrigin = startOrigin + Vector3.up * states.vaultOverHeight;
                    if (!states.run)
                        vOrigin += wallDirection * Statics.vaultCheckDistance;
                    else
                        vOrigin += wallDirection * Statics.vaultCheckDistance_Run;

                    Debug.DrawRay(vOrigin, -Vector3.up * Statics.vaultCheckDistance);

                    if (Physics.Raycast(vOrigin, -Vector3.up, out vHit, Statics.vaultCheckDistance, states.ignoreLayers))
                    {
                        float hitY = vHit.point.y;
                        float diff = hitY - transform.position.y;

                        if (Mathf.Abs(diff) < states.vaultFloorHeightDiffernce)
                        {
                            float offset = Statics.climbUpStartPosOffset;
                            if (states.inCover)
                                offset = 1;

                            Vector3 startPos = hit.normal * offset;
                            startPos = hit.point + startPos;
                            startPos.y = transform.position.y;
                            states.startVaultPosition = startPos;
                            states.curVaultType = StateManager.VaultType.walk;
                            if(states.run)
                                states.curVaultType = StateManager.VaultType.run;
                            states.canVault = true;
                            states.targetVaultPosition = vHit.point;
                        }
                    }
                }
            }
        }

        void ClimbOver(RaycastHit hit)
        {
            float targetDistance = states.distanceToCheckForward + 0.1f;

            if (states.run)
                targetDistance += 0.5f;

            Vector3 climbCheckOrigin = transform.position;
            climbCheckOrigin += Vector3.up * Statics.climbMaxHeight;
          
            RaycastHit climbHit;

            Vector3 wallDirection = -hit.normal * targetDistance;
            // Debug.DrawRay(climbCheckOrigin, wallDirection, Color.yellow);
            if (Physics.Raycast(climbCheckOrigin, wallDirection, out climbHit, 1.2f, states.ignoreLayers))
            {

            }
            else
            {
                Vector3 origin2 = hit.point;
                origin2.y = transform.position.y;
                origin2 += Vector3.up * Statics.climbMaxHeight;         
                origin2 += wallDirection * 0.2f;
                //  Debug.DrawRay(origin2, -Vector3.up, Color.yellow);

                if (Physics.Raycast(origin2, -Vector3.up, out climbHit, 1, states.ignoreLayers))
                {
                    float diff = climbHit.point.y - transform.position.y;
                    if (Mathf.Abs(diff) > Statics.walkUpThreshold)
                    {
                        states.targetVaultPosition = climbHit.point;
                        states.curVaultType = StateManager.VaultType.climb_up;    
                        Vector3 startPos = hit.normal * Statics.climbUpStartPosOffset;
                        startPos = hit.point + startPos;
                        startPos.y = transform.position.y;
                        states.startVaultPosition = startPos;
                        //states.climbAnimName = Statics.climb_up;
                        states.canVault = true;
                    }
                }
            }
        }

        bool initAim;
        Vector3 coverNormal;

        void CoverMovement()
        {
            relativeInput.x = states.horizontal;
            relativeInput.z = states.vertical;
            canAim = false;

            bool halfCover = isHalfCover();

            if (halfCover)
                states.crouching = true;

            if (relativeInput.x != 0)
            {
                input_h.camManager.leftPivot = (relativeInput.x < 0);
                movePositive = (relativeInput.x > 0);
                coverDirection = (movePositive) ? 1 : -1;
            }

            states.coverDirection = coverDirection;//for reflection purposes and future references

            bool isCover = CanMoveOnSide(movePositive);

            if (twoPointValidation)
            {
                if (!isCover)
                    isCover = CanMoveOnSide(movePositive, 0.1f);
            }

            input_h.coverNormal = coverNormal;

            Vector3 targetDir = (helper.position - transform.position).normalized;

            if (!isCover)
            {
                targetDir = Vector3.zero;
                relativeInput.x = 0;
                canAim = true;
            }

            if (halfCover)
                canAim = true;

            states.inCoverCanAim = canAim;

            if (canAim && states.aiming)
            {
                states.crouching = false;
                relativeInput = Vector3.zero;

                if (!initAim)
                {
                    float multiplier = (coverDirection > 0) ? 0.5f : 0.8f;
                    if (halfCover)
                        multiplier *= states.horizontal;

                    _targetPos = transform.position + ((helper.right * multiplier) * coverDirection);
                    _targetPos.y = transform.position.y;
                    _startPos = transform.position;                  
                    _t = 0;
                    initAim = true;
                    _initLerp = false;
                    states.rBody.isKinematic = true;
                }

                _t += Time.deltaTime * 3;

                if (_t > 1)
                    _t = 1;

                transform.position = Vector3.Lerp(_startPos, _targetPos, _t);

                targetDir = (states.aimPosition - transform.position).normalized;
                targetDir.y = 0;
                if (targetDir == Vector3.zero)
                    targetDir = transform.forward;
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
            }
            else
            {
                if (initAim)
                {
                    if(!_initLerp)
                    {
                        _targetPos = _startPos;
                        _startPos = transform.position;
                       _initLerp = true;
                        states.rBody.isKinematic = true;
                        _t = 0;
                    }

                    _t += Time.deltaTime * 3;

                    if (_t > 1)
                    {
                        _t = 1;
                        initAim = false;
                        _initLerp = false;
                        states.rBody.isKinematic = false;
                    }

                    transform.position = Vector3.Lerp(_startPos, _targetPos, _t);
                    Quaternion targetRot = Quaternion.LookRotation(helper.right * coverDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
                }
                else
                {
                    RaycastHit hit;
                    Vector3 coverDir = transform.right * -coverDirection;
                    Vector3 origin = transform.position + Vector3.up * 0.9f;
                    Debug.DrawRay(origin, coverDir);
                    if (Physics.Raycast(origin, coverDir, out hit, 1, states.ignoreLayers))
                    {
                        Vector3 dir = hit.point - origin;
                        c.coverText.position = origin + dir * 0.95f;
                        c.coverText.LookAt(hit.point + (-hit.normal * 5));
                        CanVaultOver(hit);
                        if (!states.canVault)
                            ClimbOver(hit);
                    }

                    targetDir.y = 0;
                    states.rBody.velocity = (targetDir * Mathf.Abs(relativeInput.x)) * coverMovementSpeed;

                    if (targetDir == Vector3.zero)
                        targetDir = transform.forward;
                    Quaternion targetRot = Quaternion.LookRotation(helper.right * coverDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
                }
            }


            HandleCoverAnimations(relativeInput);
        }

        void HandleCoverAnimations(Vector3 relativeInput)
        {
            float movement = Mathf.Abs(relativeInput.x);

            movement = Mathf.Clamp(movement, 0, 0.5f);
            states.anim.SetBool(Statics.crouch_anim, states.crouching);
            states.anim.SetFloat(Statics.vertical, movement, 0.3f, Time.deltaTime);
            states.anim.SetFloat(Statics.horizontal, 0, 0.3f, Time.deltaTime);
            states.anim.SetFloat(Statics.turn, 0, 0.3f, Time.deltaTime);
        }

        bool CanMoveOnSide(bool right, float offset = 0)
        {
            bool retVal = false;

            Vector3 side = (right) ? helper.right : -helper.right;
            side *= sampleDis + offset;
            Vector3 origin = transform.position + side;
            origin += Vector3.up / 2;
            Vector3 direction = helper.transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(origin, side, out hit, minCover, states.ignoreLayers))
            {
                return false;
            }
            else
            {
                RaycastHit towards;
                origin += side;

                if (Physics.Raycast(origin, direction, out towards, 1, states.ignoreLayers))
                {
                    coverNormal = towards.normal;

                    //if we hit a collider that means it's a viable cover position from this side
                    if (towards.transform.GetComponent<BoxCollider>())
                    {
                        float angle = Vector3.Angle(helper.forward, -towards.normal);

                        if (angle < 45)
                        {
                            retVal = true;
                            helper.position = PosWithOffset(origin, towards.point);
                            helper.rotation = Quaternion.LookRotation(-towards.normal);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return retVal;
        }

        bool isCoverValid(Transform h, bool right)
        {
            bool retVal = false;

            Vector3 side = (right) ? h.right : -h.right;
            side *= minCover;
            Vector3 origin = h.transform.position + side + -h.transform.forward;
            Vector3 direction = h.transform.forward;
            RaycastHit hit;

//            Debug.DrawRay(origin, direction * 2);

            if (Physics.Raycast(origin, side, out hit, minCover, states.ignoreLayers))
            {
                //if there's an obstacle on the left or right, the cover is invalid
                return false;
            }
            else//if not then do another raycast to determin the size of the collider
            {
                RaycastHit towards;

                origin += side;

                if (Physics.Raycast(origin, direction, out towards, aheadDistance, states.ignoreLayers))
                {
                    //if we hit a collider that means it's a viable cover position from this side
                    if (towards.transform.GetComponent<BoxCollider>())
                    {
                        retVal = true;

                        if (right)
                            cp.pos2 = PosWithOffset(origin, towards.point);
                        else
                            cp.pos1 = PosWithOffset(origin, towards.point);
                    }
                }
                else
                {
                    return false;
                }
            }

            return retVal;
        }

        bool isHalfCover()
        {
            bool r = true;
            
            RaycastHit hit;
            Vector3 direction = helper.forward;//
            Vector3 origin = helper.position + Vector3.up * 0.8f;
            Debug.DrawRay(origin, direction * 1, Color.blue);
            if(Physics.Raycast(origin,direction,out hit,5,states.ignoreLayers))
            {
                r = false;
            }

            return r;

        }

        Vector3 PosWithOffset(Vector3 origin, Vector3 target)
        {
            Vector3 direction = origin - target;
            direction.Normalize();
            Vector3 offset = direction * offsetFromWall;
            Vector3 retVal = target + offset;
            return retVal;
        }
      
        void GetInCoverLerp()
        {
            if (!_initLerp)
            {
                _length = Vector3.Distance(cp.pos1, cp.pos2);
                float hitDistance = Vector3.Distance(cp.initialHit, cp.pos1);
                float coverPerc = hitDistance / _length;
                _targetPos = Vector3.Lerp(cp.pos1, cp.pos2, coverPerc);
                _startPos = transform.position;
                _initLerp = true;
                _t = 0;

                coverDirection = (input_h.camManager.leftPivot) ? -1 : 1;

                states.coverDirection = 1;
                //states.rBody.isKinematic = true;
            }

            states.anim.SetFloat(Statics.vertical, 1, .3f ,Time.deltaTime);

            float movement = _lpSpeed * Time.deltaTime;
            _t += movement;

            if (_t > 1)
            {
                _t = 1;
                initCover = true;
            }

            Vector3 tp = Vector3.Lerp(_startPos, _targetPos, _t);
            tp.y = transform.position.y;
            transform.position = tp;

            Quaternion targetRot = Quaternion.LookRotation(
                (states.coverDirection > 0)?helper.transform.right
                : -helper.transform.right);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime *1);
        }

        public void ResetCover()
        {
            _initLerp = false;
            initCover = false;
            initAim = false;
        }

    }

    public class CoverPosition
    {
        public Vector3 pos1;
        public Vector3 pos2;
        public Vector3 initialHit;
    }
}
