
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Cover
{
    public class CoverBehaviour : MonoBehaviour
    {
        StateManager states;

        public bool searchForCover;
        public float aheadDistance = 3;
        bool initCover;  
        [HideInInspector]
        public bool hasCover;
        CoverPosition cp;
        float minCover;
        float offsetFromWall;
        float sampleDis;
        Transform helper;
        LayerMask ignoreLayers;

        bool _initLerp;
        float _length;
        Vector3 _startPos;
        Vector3 _targetPos;
        float _lpSpeed = 2;
        float _t;

        bool movePositive;

        public float coverMovementSpeed = 8;
        public bool twoPointValidation;

        public bool debugCover;
        GameObject debugCube;

        bool crouchCover;

        public Vector3 relativeInput;

        public void Init(StateManager st)
        {
            states = st;
            cp = new CoverPosition();

            helper = new GameObject().transform;
            helper.name = "cover helper";

            ignoreLayers = ~(1 << gameObject.layer | 1 << 3 | 1 << 10);

            if (debugCover)
            {
                debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 scale = Vector3.one * 0.2f;
                debugCube.transform.localScale = scale;         
                Destroy(debugCube.GetComponent<BoxCollider>());
            }

            minCover = 0.2f;
            offsetFromWall = 0.6f;
            sampleDis = 0.25f;
        }

        public void Tick()
        {
            if(searchForCover)
            {
                if(!hasCover)
                {
                    if(!states.aiming)
                        RaycastForCover();
                }
                else
                {
                    //Debug
                    if (debugCover)
                    {
                        debugCube.transform.position = helper.position;
                        debugCube.transform.rotation = helper.rotation;                
                    }

                    if(!initCover)
                    {
                        DisableController();
                        GetInCoverLerp();                            
                    }
                    else
                    {
                        states.handleShooting.Tick();
                        states.ikHandler.Tick();
                        states.audioManager.Tick();
                        states.weaponManager.Tick();
                        HandleCoverMovement();
                    }
                }
            }
            else
            {
              //tps controller is active
            }
        }

        void GetInCoverLerp()
        {
            if(!_initLerp)
            {
                _length = Vector3.Distance(cp.pos1, cp.pos2);
                float hitDistance = Vector3.Distance(cp.initialHit, cp.pos1);
                float coverPerc = hitDistance / _length;
                _targetPos = Vector3.Lerp(cp.pos1,cp.pos2,coverPerc);            
                _startPos = transform.position;
                _initLerp = true;
                _t = 0;

                crouchCover = !isCoverFull();
                states.crouching = crouchCover;
                states.coverDirection = 1;
            }

            float movement = _lpSpeed * states.myDelta;
            float lerpMovement = movement / _length;
            _t += movement;

            if(_t > 1)
            {
                _t = 1;
                initCover = true;
            }

            Vector3 tp = Vector3.Lerp(_startPos, _targetPos, _t);
            tp.y = transform.position.y;
            transform.position = tp;

            Quaternion targetRot = Quaternion.LookRotation(helper.transform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _t);
        }

        void RaycastForCover()
        {
            Vector3 origin = transform.position + Vector3.up/2 + -transform.forward;
            Vector3 direction = transform.forward;
            RaycastHit hit;
            float distance = aheadDistance;

            if(Physics.Raycast(origin,direction,out hit, distance, ignoreLayers))
            {
                //we hit a box collider
                if(hit.transform.GetComponent<BoxCollider>())
                {
                    Vector3 dir2= hit.transform.position - transform.position;

                    float dot = Vector3.Dot(transform.forward, dir2);

                    if (dot > 0)
                    {
                        helper.transform.position = PosWithOffset(origin, hit.point);
                        helper.transform.rotation = Quaternion.LookRotation(-hit.normal);

                        bool right = isCoverValid(helper, true);
                        bool left = isCoverValid(helper, false);

                        //the cover is the minimum size
                        if (right && left)
                        {
                            hasCover = true;
                            cp.initialHit = hit.point;
                        }
                    }
                }
            }

        }

        Vector3 PosWithOffset(Vector3 origin, Vector3 target)
        {
            Vector3 direction = origin - target;
            direction.Normalize();
            Vector3 offset = direction * offsetFromWall;
            Vector3 retVal = target + offset;
            return retVal;
        }

        bool isCoverValid(Transform h, bool right)
        {
            bool retVal = false;

            Vector3 side = (right) ? h.right: -h.right ;
            side *= minCover;
            Vector3 origin = h.transform.position + side + -h.transform.forward;
            Vector3 direction = h.transform.forward;
            RaycastHit hit;

            Debug.DrawRay(origin, direction * 2);

            if (Physics.Raycast(origin, side, out hit, minCover, ignoreLayers))
            {
                //if there's an obstacle on the left or right, the cover is invalid
                return false;
            }
            else//if not then do another raycast to determin the size of the collider
            {
                RaycastHit towards;

                origin += side;

                if(Physics.Raycast(origin, direction, out towards, aheadDistance, ignoreLayers))
                {
                    //if we hit a collider that means it's a viable cover position from this side
                    if (towards.transform.GetComponent<BoxCollider>())
                    {
                        retVal = true;

                        if(right)
                        {
                            cp.pos2 = PosWithOffset(origin, towards.point);
                        }
                        else
                        {
                            cp.pos1 = PosWithOffset(origin, towards.point); 
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

        void HandleCoverMovement()
        {
            relativeInput.x = states.horizontal;
            relativeInput.z = states.vertical;

            if(relativeInput.z < 0)
            {
                EnableController();               
                return;
            }

            if (relativeInput.x != 0)
            {
                movePositive = (relativeInput.x > 0);
                states.coverDirection = (movePositive) ? 1 : -1;
                crouchCover = !isCoverFull();

                if (crouchCover)
                {
                    states.crouching = crouchCover;
                }      
            }

            bool isCover = CanMoveOnSide(movePositive);

            if (twoPointValidation)
            {
                if(!isCover)
                    isCover = CanMoveOnSide(movePositive, 0.1f);
            }

            Vector3 targetDir = (helper.position - transform.position).normalized;
            targetDir *= Mathf.Abs(relativeInput.x);

            if (!isCover)
            {
                targetDir = Vector3.zero;
                relativeInput.x = 0;
            }

            states.aimAtSides = !isCover;
            states.canAim = states.aimAtSides;

            states.cMovement.rb.AddForce(targetDir * coverMovementSpeed);
            Quaternion targetRot = helper.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, states.myDelta * 5);
            HandleCoverAnim(relativeInput);
        }
        
        bool CanMoveOnSide(bool right , float offset = 0)
        {
            bool retVal = false;

            Vector3 side = (right) ? helper.right : -helper.right;
            side *= sampleDis + offset;
            Vector3 origin = transform.position + side;
            origin += Vector3.up / 2;       
            Vector3 direction = helper.transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(origin, side, out hit, minCover, ignoreLayers))
            {             
                return false;
            }
            else
            {
                RaycastHit towards;
                origin += side;

                if (Physics.Raycast(origin, direction, out towards, 1 , ignoreLayers))
                {
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

        bool isCoverFull()
        {
            bool retVal = false;

            Vector3 origin = helper.position + Vector3.up;
            Vector3 direction = helper.forward;
            RaycastHit hit;

            if(Physics.Raycast(origin,direction,out hit, 1, ignoreLayers))
            {
                if (hit.transform.GetComponent<BoxCollider>())
                {
                    retVal = true;
                }
            }

            states.crouchCover = !retVal;
            return retVal;
        }

        void HandleCoverAnim(Vector3 input)
        {
            states.handleAnim.anim.SetFloat("Forward", Mathf.Abs(input.x), 0.3f, states.myDelta);
            states.handleAnim.anim.SetBool("Cover", states.inCover);
            states.handleAnim.anim.SetInteger("CoverDirection", states.coverDirection);  
            states.handleAnim.anim.SetBool("CrouchToUpAim", states.crouchCover);
            states.handleAnim.Tick_Common();
        }

        IEnumerator EnableSearchForCover()
        {
            yield return new WaitForSeconds(0.3f);
            searchForCover = true;
        }

        void EnableController()
        {
            _initLerp = false;
            initCover = false;
            states.dummyModel = false;
            GetComponent<Collider>().isTrigger = false;
            states.cMovement.rb.constraints = RigidbodyConstraints.None;
            states.cMovement.rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
            hasCover = false;
            searchForCover = false;
            StartCoroutine("EnableSearchForCover");

            states.inCover = false;

            states.handleAnim.anim.SetBool("Cover", states.inCover);
            states.handleAnim.anim.SetInteger("CoverDirection", states.coverDirection);
            states.handleAnim.anim.SetBool("CrouchToUpAim", states.crouchCover);
            states.handleAnim.Tick_Common();
        }

        void DisableController()
        {
            states.dummyModel = true;
            GetComponent<Collider>().isTrigger = true;
            states.cMovement.rb.constraints = RigidbodyConstraints.FreezePositionY;
            states.inCover = true;
        }
    }

    public class CoverPosition
    {
        public Vector3 pos1;
        public Vector3 pos2;
        public Vector3 initialHit;
    }
}
