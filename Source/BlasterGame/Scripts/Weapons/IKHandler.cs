using UnityEngine;
using System.Collections;
using TPC;

namespace Weapons
{
    public class IKHandler : MonoBehaviour
    {
        Transform off_hand_idle;
        Transform off_hand_aim;
        Transform main_hand;
        Transform head_target;
        Transform headHelper;

        public bool closeAll;
        public Transform aimPivot;
        Transform targetShoulder;
        Transform aimHelper;

        Animator anim;
        StateManager states;
        Vector3 rs_offset;

        float m_weight;
        float t_m_weight;
        float o_weight;
        float t_o_weight;
        float weightSpeed = 3;
        float head_Weight;
        float body_Weight;
        float weightT;
        bool closeIK;
        bool closeHeadIK;
        bool useHeadTarget;
        Vector3 curHeadPos;

        public void Init(StateManager st)
        {
            states = st;
            anim = st.anim;

            CreateHelpers();
            CreateAimHelper();

            m_weight = 0;
            o_weight = 1;

            curHeadPos = transform.position + (transform.forward * 25) + Vector3.up;
            states.aimPosition = curHeadPos;
        }

        public void LoadWeapon(RuntimeWeapon rw)
        {
            SetHeadPosition(rw.activeStats.headTargetPos);
            anim.SetInteger(Statics.weaponType, rw.wReference.weaponAnimSet);
            useHeadTarget = rw.activeStats.useHeadTarget;
        }

        public void Tick()
        {
            HandleWeights();
            RecoilActual();
            states.bHelpers.Tick();
        }

        public void LateTick()
        {
            states.bHelpers.Tick();
        }

        bool CanAim()
        {
            if (anim.GetBool(Statics.onLocomotion) == false)
                return false;

            Vector3 targetDir = (states.aimPosition - transform.position).normalized;
            targetDir.y = 0;
            float turnAngle = Vector3.Angle(transform.forward, targetDir);
            if (turnAngle > 25)
                return false;

            RaycastHit hit;
            Debug.DrawRay(aimPivot.position, aimPivot.forward * 1, Color.red);
            if(Physics.Raycast(aimPivot.position,aimPivot.forward, out hit,1, states.ignoreLayers))
            {
                return false;
            }

            return true;       
        }

        void HandleWeights()
        {
            float headTargetWeight = 0;
            float bodyTargetWeight = 0;
            float multiplier = 1;

            closeIK = anim.GetBool(Statics.closeIK);
            closeHeadIK = anim.GetBool(Statics.closeHeadIK);

            states.canAim = CanAim();
            
            if (states.aiming && states.canAim)
            {
                headTargetWeight = 1;
                t_m_weight = 1;
                bodyTargetWeight = 0;
            }
            else
            {              
                if (!states.run)
                {
                    headTargetWeight =1;
                    bodyTargetWeight = 0.15f;
                }
                else
                {
                    headTargetWeight = 0;
                    bodyTargetWeight = 0;
                }

                t_m_weight = 0;
                multiplier = 2;
            }

            if(states.inCover)
            {
                headTargetWeight = 0;
                bodyTargetWeight = 0;

                if(states.aiming)
                {
                    headTargetWeight = 1;
                    bodyTargetWeight = 0;
                }
            }

            if(closeIK)
            {               
                bodyTargetWeight = 0;
                t_m_weight = 0;
                t_o_weight = 0;
                multiplier = 5;
            }
            else
            {
                t_o_weight = 1;
            }

            if(closeHeadIK)
            {
                headTargetWeight = 0;
            }

            if(closeAll || states.vaulting)
            {
                t_m_weight = 0;
                headTargetWeight = 0;
                bodyTargetWeight = 0;
            }

            weightT = Time.deltaTime * weightSpeed * multiplier;

            m_weight = Mathf.Lerp(m_weight, t_m_weight, weightT * multiplier);
            o_weight = Mathf.Lerp(o_weight, t_o_weight, weightT * multiplier);

            head_Weight = Mathf.Lerp(head_Weight, headTargetWeight, weightT);
            body_Weight = Mathf.Lerp(body_Weight, bodyTargetWeight, weightT);           
        }

        void OnAnimatorMove()
        {
            HandleShoulder();
        }

        void HandleShoulder()
        {
            HandleShoulderPosition();
            HandleShoulderRotation(states.aimPosition);

            Vector3 h_target = states.aimPosition;
            if (states.aiming)
            {
                h_target = head_target.position;
            }

            Vector3 targetP = Vector3.Lerp(headHelper.position, h_target, Time.deltaTime * 4);
            headHelper.position = targetP;
        }

        void CreateHelpers()
        {
            targetShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
            aimPivot = new GameObject().transform;
            aimPivot.name = "aim pivot";
            main_hand = new GameObject().transform;
            main_hand.name = "main_hand";
            off_hand_idle = new GameObject().transform;
            off_hand_idle.name = "off_hand_idle";
            off_hand_aim = new GameObject().transform;
            off_hand_aim.name = "off_hand_aim";
            head_target = new GameObject().transform;
            head_target.name = "head_target";
            head_target.transform.parent = aimPivot;
            headHelper = new GameObject().transform;
            headHelper.name = "head_helper";

            aimPivot.transform.position = targetShoulder.transform.position;
            main_hand.parent = aimPivot;
            main_hand.transform.localPosition = new Vector3(0.18f, 0.017f, 0.305f);
            main_hand.transform.localEulerAngles = new Vector3(0.726f, 0.309f, -84.783f);

            off_hand_idle.parent = anim.GetBoneTransform(HumanBodyBones.RightHand);
            off_hand_idle.localPosition = new Vector3(0.05f, 0.1386f, -0.0219f);
            off_hand_idle.localEulerAngles = new Vector3(-13.79f, -66.288f, 92.591f);

            off_hand_aim.parent = aimPivot;
            off_hand_aim.localPosition = new Vector3(0.143f, 0.131f, .672f);
            off_hand_aim.localRotation = off_hand_idle.localRotation;
        }

        void CreateAimHelper()
        {
            aimHelper = new GameObject().transform;
            aimHelper.name = "aim helper";
        }

        float currentY;
        float currentZ;

        public void HandleShoulderPosition()
        {
            Vector3 f = transform.forward * rs_offset.z;
            Vector3 r = transform.right * rs_offset.x;
            Vector3 u = transform.up * rs_offset.y;
            Vector3 finalPosition = (f + r + u) + targetShoulder.position;
            aimPivot.transform.position = finalPosition;

            int sign = Vector3.Cross(transform.forward, aimPivot.transform.forward).z < 0 ? -1 : 1;
            float currentAngle = Vector3.Angle(transform.forward, aimPivot.transform.forward);
            float scale = currentAngle / 35;
            currentAngle *= sign;
            currentY = scale * states.weaponManager.GetActive().activeStats.offsetYscale;
            currentZ = scale * states.weaponManager.GetActive().activeStats.offsetZscale;
            Vector3 yOffset = aimPivot.up * -currentY;
            Vector3 zOffset = aimPivot.forward * currentZ;

            aimPivot.transform.position += (yOffset + zOffset);
        }

        void HandleShoulderRotation(Vector3 targetPos)
        {
            aimHelper.transform.position = Vector3.Lerp(aimHelper.transform.position,
            targetPos, Time.deltaTime * Statics.aimHelperSpeed);

            Vector3 targetDir = (aimHelper.transform.position - aimPivot.position).normalized;
            if (targetDir == Vector3.zero)
                targetDir = aimPivot.forward;
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            aimPivot.rotation = Quaternion.Slerp(aimPivot.rotation,
                targetRot, Time.deltaTime * Statics.shoulderRotateSpeed);
        }

        void OnAnimatorIK()
        {
            anim.SetLookAtWeight(1, body_Weight, head_Weight, 1, 1);

            curHeadPos = headHelper.position;
            anim.SetLookAtPosition(curHeadPos);

           MainHandIK(AvatarIKGoal.RightHand, m_weight);
            OffHandIK(AvatarIKGoal.LeftHand, o_weight);
        }

        void MainHandIK(AvatarIKGoal goal, float weight)
        {
            anim.SetIKPositionWeight(goal, weight);
            anim.SetIKPosition(goal, main_hand.position);
            anim.SetIKRotationWeight(goal, weight);
            anim.SetIKRotation(goal, main_hand.rotation);
        }

        void OffHandIK(AvatarIKGoal goal, float weight)
        {
            anim.SetIKPositionWeight(goal, weight);
            anim.SetIKPosition(goal, off_hand_idle.position);
            anim.SetIKRotationWeight(goal, weight);
            anim.SetIKRotation(goal, off_hand_idle.rotation);
        }

        public void SetHeadPosition(Vector3 pos)
        {
            head_target.transform.localPosition = pos;
        }

        public void SetHeadWeight(float weight)
        {
            anim.SetLookAtWeight(weight, 0, 1, 1, 1);
            anim.SetLookAtPosition(head_target.transform.position);
        }

        public void SetMainHandPos(Vector3 pos)
        {
            main_hand.localPosition = pos;
        }

        public void SetMainHandRot(Vector3 rot)
        {
            main_hand.localEulerAngles = rot;
        }

        public void SetOffHandPos(Vector3 pos, Transform parent)
        {
            off_hand_idle.parent = parent;
            off_hand_idle.localPosition = pos;
        }

        public void SetOffHandAimPos(Vector3 pos)
        {
            off_hand_aim.localPosition = pos;
        }

        public void SetShoulderOffset(Vector3 pos)
        {
            rs_offset = pos;
        }

        public void SetOffHandRot(bool aim, Vector3 rot)
        {
            if (aim)
            {
                off_hand_aim.localEulerAngles = rot;
            }else
            {
                off_hand_idle.localEulerAngles = rot;
            }
        }

        #region Recoil
        float recoilT;
        Vector3 offsetPosition;
        Vector3 offsetRotation;
        Vector3 basePosition;
        Vector3 baseRotation;
        bool recoilIsInit;
        RuntimeWeapon activeWeapon;
        
        public void RecoilAnim(RuntimeWeapon rw)
        {
            if (!recoilIsInit)
            {
                recoilT = 0;
                offsetPosition = Vector3.zero;
                recoilIsInit = true;
                basePosition = main_hand.localPosition;
                baseRotation = main_hand.localEulerAngles;
                activeWeapon = rw;
            }
        }

        void RecoilActual()
        {
            if (recoilIsInit)
            {
                recoilT += Time.deltaTime * 3;
                if (recoilT > 1)
                {
                    recoilT = 1;
                    recoilIsInit = false;
                }

                Vector3 yDir = Vector3.up * activeWeapon.activeStats.recoilY.Evaluate(recoilT);
                Vector3 zDir = Vector3.forward * activeWeapon.activeStats.recoilZ.Evaluate(recoilT);
                offsetRotation = Vector3.right * 90 * -activeWeapon.activeStats.angularX.Evaluate(recoilT);
                offsetPosition = yDir + zDir;

                main_hand.localPosition = basePosition + offsetPosition;
                main_hand.localEulerAngles = baseRotation + offsetRotation;
            }
        }

        #endregion

    }
}
