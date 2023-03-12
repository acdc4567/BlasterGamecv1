using UnityEngine;
using System.Collections;

namespace DualWield
{
    public class Akimbo : MonoBehaviour
    {
        public bool enableAkimbo;
        Animator anim;
        public Transform leftShoulder;
        StateManager states;
        IKHandler ikHandler;
        HandleShooting hShooting;
        public Transform lh_weaponHolder;
        public WeaponReferenceBase akimboWeapon;

        float targetWeight;
        float lh_weight;

        Transform aimHelperLh;
        bool disableIK;

        Transform leftHandIKTarget;
        Transform leftHandIKRotation;

        bool waitToShoot;

        [HideInInspector]
        public AudioSource akimboAudioSource;

        Vector3 targetPosition;
        bool holdTarget;

        public void Init(StateManager st)
        {
            this.states = st;
            anim = states.handleAnim.anim;
            ikHandler = states.ikHandler;
            hShooting = states.handleShooting;
            leftShoulder = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
            aimHelperLh = ikHandler.aimHelperLS;

            leftHandIKTarget = akimboWeapon.rightHandTarget;
            leftHandIKRotation = akimboWeapon.rightHandRotation;

            GameObject audioGO = Instantiate(states.audioManager.gunSounds.gameObject);
            akimboAudioSource = audioGO.GetComponent<AudioSource>();
            audioGO.transform.parent = transform;
            audioGO.transform.localPosition = Vector3.zero;
            akimboAudioSource.clip = akimboWeapon.weaponStats.shootSound;

            DisableAkimbo();
        }

        public void ReInit()
        {
            leftShoulder = anim.GetBoneTransform(HumanBodyBones.LeftShoulder);
        }

        public void EnableAkimbo()
        {
            enableAkimbo = true;

            anim.SetBool("Akimbo", true);

            if (akimboWeapon.weaponModel)
                akimboWeapon.weaponModel.SetActive(true);

            if (akimboWeapon.ikHolder)
                akimboWeapon.ikHolder.SetActive(true);

        }

        public void DisableAkimbo()
        {
            enableAkimbo = false;

            if (akimboWeapon.weaponModel)
                akimboWeapon.weaponModel.SetActive(false);

            if (akimboWeapon.ikHolder)
                akimboWeapon.ikHolder.SetActive(false);

            anim.SetBool("Akimbo", false);
        }

        public void Tick()
        {
            holdTarget = Input.GetKey(KeyCode.LeftAlt);

            if (enableAkimbo)
            {
                if (!holdTarget)
                {
                    targetPosition = states.lookHitPosition;
                    akimboWeapon.aimPosition = targetPosition;
                }

                HandleShoulders();
                HandleShoulderRotation();
                AimWeight();
                hShooting.HandleModelAnimator(akimboWeapon);

                if (states.shoot && !states.reloading && states.aiming)
                {
                    if(!waitToShoot && targetWeight > 0)
                    {
                        Invoke("AkimboShooting", 0.2f);
                        waitToShoot = true;
                    }
                }  
            }
        }

        void AkimboShooting()
        {                      
            hShooting.ActualShooting(akimboWeapon);
            waitToShoot = false;
        }

        void HandleShoulders()
        {
            if (leftShoulder == null)
            {
                leftShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
            }
            else
            {
                lh_weaponHolder.position = leftShoulder.transform.position;
            }
        }

        void AimWeight()
        {
            if (states.aiming && !states.reloading && !states.vaulting && states.onGround)
            {
                Vector3 directionTowardsTarget = aimHelperLh.position - transform.position;
                float angleFromForward = Vector3.Angle(transform.forward, directionTowardsTarget);
                float angleFromLeft = Vector3.Angle(-transform.right, directionTowardsTarget);

                float targetAngle = (holdTarget) ? 50 : 90 ;

                if (angleFromForward < targetAngle)
                {
                    if (!holdTarget)
                    {
                        targetWeight = 1;
                    }
                    else
                    {
                        if(angleFromLeft < 100)
                        {
                            targetWeight = 1;
                        }
                        else
                        {
                            targetWeight = 0;
                        }
                    }
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

            if (disableIK)
            {
                targetWeight = 0;
            }

            lh_weight = Mathf.Lerp(lh_weight, targetWeight,
                states.myDelta * multiplier);
        }

        void HandleShoulderRotation()
        {
            aimHelperLh.position = Vector3.Lerp(aimHelperLh.position, targetPosition, states.myDelta * 6);
            lh_weaponHolder.LookAt(aimHelperLh.position);
            leftHandIKTarget.parent.transform.LookAt(aimHelperLh.position);
            leftHandIKTarget.transform.LookAt(aimHelperLh.position);
        }

        void OnAnimatorIK()
        {
            if (enableAkimbo)
            {
                if (leftHandIKTarget)
                {
                    if (leftHandIKRotation == null)
                        leftHandIKRotation = leftHandIKTarget;

                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, lh_weight);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, lh_weight);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKRotation.rotation);
                }
                else
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
        }


    }
}
