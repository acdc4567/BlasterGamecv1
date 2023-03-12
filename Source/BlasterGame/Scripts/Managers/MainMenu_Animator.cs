using UnityEngine;
using System.Collections;
using TPC;
using Weapons;
using Manager;

namespace Manager.Menu
{
    public class MainMenu_Animator : MonoBehaviour
    {
        Animator anim;
        BoneHelpers bHelper;
        Transform lh_ikHelper;
        RuntimeWeapon runtimeWeapon;
        string rigType;

        public void Init(string targetRig)
        {
            rigType = targetRig;
            bHelper = GetComponent<BoneHelpers>();
            anim = GetComponent<Animator>();
            bHelper.Init(anim);
            LoadWeapons();

            bHelper.ParentAllHelpers();
        }

        void LoadWeapons()
        {
            PlayerProfile pf = SessionMaster.singleton.GetProfile();
            ResourcesManager rm = ResourcesManager.singleton;

            if (string.IsNullOrEmpty(pf.mainWeapon))
                pf.mainWeapon = "m4";
            if (string.IsNullOrEmpty(pf.secWeapon))
                pf.secWeapon = "b8";

            WeaponInstance mw = rm.GetWeapon(pf.mainWeapon, rigType);
            WeaponInstance sw = rm.GetWeapon(pf.secWeapon, rigType);

            if (mw == null) //in case we can't find the weapon, fall back to a default one
            {
               // Debug.Log(pf.mainWeapon + " main weapon wasn't found for "
              //      + rigType + " rig type, assigning default");

                mw = rm.GetWeapon("m4", rigType);

            }
            if (sw == null)
            {
              //  Debug.Log(pf.secWeapon + " secondary weapon wasn't found for "
              //    + rigType + " rig type, assigning default");

                sw = rm.GetWeapon("b8", rigType);
            }

            if(mw == null)
            {
               Debug.Log("default main weapon for " + rigType + " rig type, wasn't found! Fix this!");
            }

            if(sw == null)
            {
                Debug.Log("default secondary weapon for " + rigType + " rig type, wasn't found! Fix this!");
            }

            //v2 change the order so the main weapon's stats are the last to  be updated
            CreateWeapon(sw.instance, true);
            CreateWeapon(mw.instance, false);
           

            //and  the ik positions will be taken by the mod if applicable 
            lh_ikHelper = new GameObject().transform;
            lh_ikHelper.parent = bHelper.ReturnHelper(HumanBodyBones.RightHand).helper;
            lh_ikHelper.transform.localPosition = runtimeWeapon.activeStats.offHand_pos_idle;
            lh_ikHelper.transform.localEulerAngles = runtimeWeapon.activeStats.offHand_rot_idle;
        }

        void OnAnimatorIK()
        {
            if (anim != null && lh_ikHelper != null)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, lh_ikHelper.transform.position);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, lh_ikHelper.transform.rotation);
            }
        }

        void CreateWeapon(Weapon wi, bool holster)
        {
            GameObject go = Instantiate(wi.modelPrefab);

            //v2
            WeaponModelHook hook = go.GetComponent<WeaponModelHook>();
            if (hook)
            {
                runtimeWeapon = new RuntimeWeapon();
                runtimeWeapon.wReference = wi;
                runtimeWeapon.activeStats = new WeaponStats();
                Statics.CopyWeaponStatsFromWeaponInstance(ref runtimeWeapon.activeStats, wi.weaponStats);
                runtimeWeapon.modelInstance = go;
                runtimeWeapon.modelReferences = go.GetComponent<WeaponModelHook>();

              
              
            }

            //v2
            PlayerProfile pl = SessionMaster.singleton.GetProfile();
           
            if (!holster)
            {
                Transform b = bHelper.ReturnHelper(HumanBodyBones.RightHand).helper;
                go.transform.parent = b;
                go.transform.localPosition = wi.modelPos;
                go.transform.localEulerAngles = wi.modelRot;
                go.transform.localScale = wi.modelScale;
                hook.activeMods = pl.mainWeaponMods;//add the mods for the main weapon
            }
            else
            {
                Transform b = bHelper.ReturnHelper(wi.holsterBone).helper;
                go.transform.parent = b;
                go.transform.localPosition = wi.holsterPos;
                go.transform.localEulerAngles = wi.holsterRot;
                go.transform.localScale = wi.holsterScale;
                hook.activeMods = pl.secWeaponMods;//add the mods for the secondary weapon
            }

            //init the weapon
            hook.Init(bHelper, rigType, runtimeWeapon);
        }
    }
}
