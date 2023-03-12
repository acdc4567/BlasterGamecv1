using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponMod_Resources : MonoBehaviour
    {
        public List<WModBase> weaponMods = new List<WModBase>();
        Dictionary<string, int> modDictionary = new Dictionary<string, int>();
        List<WeaponMod_instance> modInstances = new List<WeaponMod_instance>();
        Dictionary<string, int> instDict = new Dictionary<string, int>();

        void Init()
        {
            for (int i = 0; i < weaponMods.Count; i++)
            {
                if(modDictionary.ContainsKey(weaponMods[i].id))
                {
                    Debug.Log("Duplicate weapon modification in the list!");
                    continue;
                }
                else
                {
                    modDictionary.Add(weaponMods[i].id, i);
                }
            }
        }

        public WModBase GetMod(string id)
        {
            WModBase r = null;
            int index = -1;

            if(modDictionary.TryGetValue(id, out index))
            {
                r = weaponMods[index];
                
            }
            return r;
        }

        public WeaponMod_instance GetModInstance(string id)
        {
            WeaponMod_instance r = null;
            int index = -1;

            if(instDict.TryGetValue(id,out index))
            {
                r = modInstances[index];
            }
            else
            {
                WModBase b = GetMod(id);

                if(b != null)
                {
                    if(b.modPrefab == null)
                    {
                        Debug.Log("Mod with id " + id + " has an empty prefab");
                        return null;
                    }

                    GameObject go = Instantiate(b.modPrefab);
                    WeaponMod_instance wi = go.GetComponent<WeaponMod_instance>();
                    modInstances.Add(wi);
                    r = wi;
                    instDict.Add(id, modInstances.Count - 1);
                    go.transform.parent = this.transform;
                }
            }

            return r;
        }

        public static WeaponMod_Resources singleton;
        void Awake()
        {
            singleton = this;
            Init();
        } 
    }

    public enum WModType
    {
        optic,barrel,rail,siderail,magazine
    }

    [System.Serializable]
    public class WModBase
    {
        public string id;
        public GameObject modPrefab;
        public WModType modType;
    }
}
