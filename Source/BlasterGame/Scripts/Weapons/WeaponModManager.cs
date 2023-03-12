using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponModManager : MonoBehaviour
    {
        WeaponModelHook wmHook;
        public List<ModContainer> activeMods = new List<ModContainer>();
        
        public void AddMod(string modId)
        {
            if (string.IsNullOrEmpty(modId))
            {
                return;
            }

            WeaponMod_Resources wmr = WeaponMod_Resources.singleton;
            WeaponMod_Vis instance = wmr.GetModInstance(modId).instance;

            ModContainer md = GetContainer(instance.modType);

            if(md == null) //there's no mod container already placed there, but does the weapon have a mod slot?
            {
                wmHook = GetComponent<WeaponModelHook>();

                WM_Place slot = wmHook.GetMod(instance.modType);

                if(slot != null)
                {
                    md = new ModContainer();
                    md.slot = instance.modType;
                    PlaceModModel(instance, modId, md);
                }
                else
                {
                    //the weapon doesn't have a slot for this mod
                }
            }
            else
            {
                //Remove previous
                ClearContainer(md);
                PlaceModModel(instance, modId, md);               
            }
        }

        public void ClearContainer(ModContainer md)
        {
            if (md.modBase)
            {
                Destroy(md.modBase);
                md.modBase.DisableOnWeapon();
            }

            if (md.modelInstance)
            {
                Destroy(md.modelInstance);
            }
           
        }

        public void ClearAllContainers()
        {
            for (int i = 0; i < activeMods.Count; i++)
            {
                ClearContainer(activeMods[i]);
            }
        }

        void PlaceModModel(WeaponMod_Vis inst, string modId, ModContainer md, WM_Place slot = null)
        {
            if(slot == null)
            {
                if(wmHook == null)
                    wmHook = GetComponent<WeaponModelHook>();

                slot = wmHook.GetMod(md.slot);
            }

            slot.modId = modId;
            md.modelInstance = Instantiate(inst.visPrefab) as GameObject;
            md.modelInstance.transform.parent = slot.place;
            md.modelInstance.transform.localPosition = inst.localPosition;
            md.modelInstance.transform.localEulerAngles = inst.localEuler;
            md.modelInstance.transform.localScale = inst.localScale;
            md.modBase = md.modelInstance.GetComponent<WeaponMod_Base>();
            if (md.modBase)
            {
                md.modBase.SetRuntime(wmHook.runtimeWeapon);
                md.modBase.EnableOnWeapon();
            }
        }

        ModContainer GetContainer(WModType type)
        {
            ModContainer r = null;
            for (int i = 0; i < activeMods.Count; i++)
            {
                if(activeMods[i].slot == type)
                {
                    r = activeMods[i];
                    break;
                }
            }

            return r;
        }


        [System.Serializable]
        public class ModContainer
        {
            public WModType slot;
            public WeaponMod_Base modBase;
            public GameObject modelInstance;
        }
    }

    
}
