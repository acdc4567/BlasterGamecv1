using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TPC;
using Manager;
using TPC.ObjectPool;

namespace Weapons
{
    public class WeaponManager : MonoBehaviour
    {
        public List<string> weapons = new List<string>();
        public int index;
        public List<RuntimeWeapon> weaponReferences = new List<RuntimeWeapon>();
        StateManager states;
        RuntimeWeapon activeWeapon;
        WaitForSeconds switchDelay;

        public RuntimeWeapon GetActive()
        {
            return activeWeapon;
        }

        public void Init(StateManager st)
        {
            states = st;
            CreateWeapons();
            switchDelay = new WaitForSeconds(Statics.delay_switch_time);
        }

        void CreateWeapons()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponInstance w = ResourcesManager.singleton.GetWeapon(weapons[i],states.modelRig);
                CreateWeaponInstances(w.instance);     
            }

            //v2
            PlayerProfile pl = SessionMaster.singleton.GetProfile();
            UpdateWeaponModsViaList(weaponReferences[0].modelReferences, pl.mainWeaponMods);
            UpdateWeaponModsViaList(weaponReferences[1].modelReferences, pl.secWeaponMods);

            SetActiveWeapon(weaponReferences[0]);
        }

        void CreateWeaponInstances(Weapon w)
        {
            GameObject prefab = w.modelPrefab;
            GameObject modelInstance = Instantiate(prefab) as GameObject;

            if (states == null)
                GetComponent<StateManager>();
       
            RuntimeWeapon rw = new RuntimeWeapon();
            WeaponStats statInstance = new WeaponStats();
            Statics.CopyWeaponStatsFromWeaponInstance(ref statInstance, w.weaponStats);
            rw.activeStats = statInstance;
            rw.curBullets = w.weaponStats.magazineBullets;
            rw.modelInstance = modelInstance;
            rw.wReference = w;

            rw.modelReferences = rw.modelInstance.GetComponent<WeaponModelHook>();
            if (rw.modelReferences == null)
            {
                rw.modelInstance.AddComponent<WeaponModelHook>();
                rw.modelReferences = rw.modelInstance.GetComponent<WeaponModelHook>();           
            }

            //v2 add the runtime weapon
            rw.modelReferences.Init(states.bHelpers, states.modelRig, rw);
            rw.modelReferences.weaponId = rw.wReference.weaponId;

            if(w.hasHolster)
            {
                if (w.holsterPrefab)
                    rw.holsterInstance = Instantiate(w.holsterPrefab) as GameObject;
                else
                    rw.holsterInstance = rw.modelInstance;
            }

            SetHolsterWeapon(rw);

            weaponReferences.Add(rw);          
        }

        public void SetHolsterWeapon(RuntimeWeapon rw)
        {
            if(rw.wReference.hasHolster)
            {
                rw.modelInstance.SetActive(false);
                rw.holsterInstance.SetActive(true);
                Transform b = states.bHelpers.ReturnHelper(rw.wReference.holsterBone).helper;
                rw.holsterInstance.transform.parent = b;
                rw.holsterInstance.transform.localPosition = rw.wReference.holsterPos;
                rw.holsterInstance.transform.localEulerAngles = rw.wReference.holsterRot;
                rw.holsterInstance.transform.localScale = rw.wReference.holsterScale;
            }
            else
            {
                rw.modelInstance.SetActive(false);
            }
        }

        public void SetActiveWeapon(RuntimeWeapon rw)
        {
            if(activeWeapon != null)
            {
                SetHolsterWeapon(activeWeapon);
            }

            HumanBodyBones targetBone = (states.mainShoulderIsLeft) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            Transform b = states.bHelpers.ReturnHelper(targetBone).helper;

            if (rw.wReference.hasHolster)
                if (rw.holsterInstance != rw.modelInstance)
                    rw.holsterInstance.SetActive(false);

            rw.modelInstance.SetActive(true);
            GameObject mi = rw.modelInstance;
            mi.transform.parent = b;
            mi.transform.localPosition = rw.wReference.modelPos;
            mi.transform.localEulerAngles = rw.wReference.modelRot;
            mi.transform.localScale = rw.wReference.modelScale;

            states.ikHandler.LoadWeapon(rw);

            states.ikHandler.SetShoulderOffset(rw.activeStats.aimguide_offset);
            states.ikHandler.SetMainHandPos(rw.activeStats.mainHand_pos);
            states.ikHandler.SetMainHandRot(rw.activeStats.mainHand_rot);   
            states.ikHandler.SetOffHandPos(rw.activeStats.offHand_pos_idle, b);
            states.ikHandler.SetOffHandRot(false, rw.activeStats.offHand_rot_idle);
            states.ikHandler.SetOffHandAimPos(rw.activeStats.offHand_pos_aim);
            states.ikHandler.SetOffHandRot(true, rw.activeStats.offHand_rot_aim);

            activeWeapon = rw;
        }

        public void Tick() //if(inAction == false)
        {
            if (states.shooting && !states.reloading)
            {
                if (activeWeapon.activeStats.frate <= 0)
                {
                    if(activeWeapon.curBullets > 0)
                        ShootWeaponActual(activeWeapon);
                }
                else
                {
                    activeWeapon.activeStats.frate -= Time.deltaTime;
                    states.actualShooting = false;
                }
            }
            else
            {
                activeWeapon.activeStats.frate -= Time.deltaTime;
                activeWeapon.activeStats.frate = Mathf.Clamp(activeWeapon.activeStats.frate,0
                    , activeWeapon.activeStats.frate);
                states.actualShooting = false;
            }
        }

        public void ReloadWeapon()
        {
            if (!states.hold)
            {
                if (activeWeapon.activeStats.curCarryBullets <= 0)
                    return;

                string animName = Statics.rifle_reload;
                if (activeWeapon.wReference.weaponAnimSet == 1)
                    animName = Statics.pistol_reload;

                states.anim.CrossFade(animName, 0.3f);

                int targetAmmo = activeWeapon.activeStats.magazineBullets - activeWeapon.curBullets;
                targetAmmo = Mathf.Clamp(targetAmmo, 0, activeWeapon.activeStats.magazineBullets);
                 
                activeWeapon.curBullets = targetAmmo + activeWeapon.curBullets;
                activeWeapon.activeStats.curCarryBullets -= targetAmmo;

                states.reloading = true;
                states.hold = true;
                StartCoroutine(Statics.DelaySwitch);
            }
        }

        public void ChangeWeapon()
        {
            if (!states.hold)
            {
                index = (index < weaponReferences.Count - 1) ? index + 1 : 0;
                SetActiveWeapon(weaponReferences[index]);

                string targetAnim = Statics.rifle_switch;
                if (weaponReferences[index].wReference.weaponAnimSet == 1)
                    targetAnim = Statics.pistol_switch;
                states.anim.Play(targetAnim);
                states.hold = true;
                states.switchingWeapon = true;
                StartCoroutine(Statics.DelaySwitch);
            }
        }

        IEnumerator DelaySwitch()
        {
            yield return switchDelay;
            states.hold = false;
        }

        void ShootWeaponActual(RuntimeWeapon rw)
        {
            Vector3 bulletOrigin = states.ikHandler.aimPivot.TransformPoint(rw.activeStats.bulletSpawnPosition);
            Vector3 direction = states.aimPosition - bulletOrigin;
            Ballistics.RayBullets(bulletOrigin, direction, rw);

            states.actualShooting = true;
            states.ikHandler.RecoilAnim(rw);
            rw.modelReferences.Fire();
            rw.activeStats.frate = rw.activeStats.fireRate;
            activeWeapon.curBullets--;        
        }

        public void PickupWeapon(string weaponID, int magBullets, int carryBullets)
        {
            WeaponInstance w = ResourcesManager.singleton.GetWeapon(weaponID, states.modelRig);

            if (w == null)
                return;

            RuntimeWeapon prev = GetActive();

            CreateWeaponInstances(w.instance);

            RuntimeWeapon newWeapon = weaponReferences[weaponReferences.Count - 1];
            SetActiveWeapon(newWeapon);

            Destroy(prev.modelInstance);
            Destroy(prev.holsterInstance);
            weaponReferences.Remove(prev);

            newWeapon.curBullets = magBullets;
            newWeapon.activeStats.curCarryBullets = carryBullets;

            newWeapon.curBullets = Mathf.Clamp(newWeapon.curBullets, 0, newWeapon.activeStats.magazineBullets);
            newWeapon.activeStats.curCarryBullets = Mathf.Clamp(newWeapon.activeStats.curCarryBullets,
                0, newWeapon.activeStats.maxCarryBullets);

            GameObject pick = ObjectPool.singleton.RequestObject(prev.wReference.pickId);
            if (pick == null)
                return;

            pick.transform.position = transform.forward + transform.position + Vector3.up *0.3f;

            TPC.Items.PickableWeapon pw = pick.GetComponent<TPC.Items.PickableWeapon>();
            pw.curBullets = prev.curBullets;
            pw.carryBullets = prev.activeStats.curCarryBullets;
        }

        //v2
        public void ReplaceWeapon(Weapons.Modifications.CarryingWeapons c)
        {
            weapons.Clear();
            weapons.Add(c.primary.weaponId);
            weapons.Add(c.secondary.weaponId);

            foreach (RuntimeWeapon r in weaponReferences)
            {
                Destroy(r.modelInstance);
                Destroy(r.holsterInstance);
            }

            weaponReferences.Clear();

            WeaponInstance prim = ResourcesManager.singleton.GetWeapon(c.primary.weaponId, states.modelRig);
            CreateWeaponInstances(prim.instance);
            WeaponInstance sec = ResourcesManager.singleton.GetWeapon(c.secondary.weaponId, states.modelRig);
            CreateWeaponInstances(sec.instance);

            UpdateWeaponMods(c.primary, weaponReferences[0].modelReferences);
            UpdateWeaponMods(c.secondary, weaponReferences[1].modelReferences);
            SetActiveWeapon(weaponReferences[0]);
        }

        public void UpdateWeaponMods(Modifications.WeaponWithModsContainer mc, WeaponModelHook wmh)
        {
            wmh.activeMods.Clear();
            wmh.activeMods.Add(mc.optic);
            wmh.activeMods.Add(mc.magazine);
            wmh.activeMods.Add(mc.rail);
            wmh.activeMods.Add(mc.barrel);
            wmh.InitMods();
        }

        public void UpdateWeaponModsViaList(WeaponModelHook wmh, List<string> modList)
        {
            wmh.activeMods.Clear();
            wmh.activeMods.AddRange(modList);
            wmh.InitMods();
        }


        #region Events

        public void W_EnableMagazineOnHand()
        {
            if (activeWeapon.modelReferences)
                activeWeapon.modelReferences.OpenMagazineOnHand();
        }

        public void W_DisableMagazineOnHand()
        {
            if (activeWeapon.modelReferences)
                activeWeapon.modelReferences.CloseMagazineOnHand();
        }

        public void W_DisableMagazineOnWeapon()
        {
            if (activeWeapon.modelReferences == null)
                return;

            if (activeWeapon.modelReferences.magazineOnWeapon == null)
                return;

            activeWeapon.modelReferences.magazineOnWeapon.SetActive(false);
            activeWeapon.modelReferences.ThrowMagazine();

        }

        public void W_EnableMagazineOnWeapon()
        {
            if (activeWeapon.modelReferences == null)
                return;

            if (activeWeapon.modelReferences.magazineOnWeapon == null)
                return;

            activeWeapon.modelReferences.magazineOnWeapon.SetActive(true);
        }

        #endregion
    }

    [System.Serializable]
    public class RuntimeWeapon
    {
        public bool isActive;
        public WeaponStats activeStats;
        public int curBullets;
        public GameObject modelInstance;
        public GameObject holsterInstance;
        public Weapon wReference;
        public WeaponModelHook modelReferences;

        //v2
        public Modifications.WeaponWithModsContainer GetModContainer()
        {
            Modifications.WeaponWithModsContainer r = new Modifications.WeaponWithModsContainer();

            r.activeWeapon = wReference;
            PopulateModString(Modifications.WModType.optic, ref r.optic);
            PopulateModString(Modifications.WModType.rail, ref r.rail);
            PopulateModString(Modifications.WModType.magazine, ref r.magazine);
            PopulateModString(Modifications.WModType.barrel, ref r.barrel);
            return r;
        }

        void PopulateModString(Modifications.WModType type, ref string modId)
        {
            Modifications.WM_Place slot = modelReferences.GetMod(type);
            if (slot != null)
                modId = slot.modId;
            else
                modId = string.Empty;
        }
    }
}
