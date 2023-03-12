using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Weapons;
using Manager;

namespace TPC
{
    public class ResourcesManager : MonoBehaviour
    {
        //Rigs
        List<RigType> rigTypes = new List<RigType>();
        Dictionary<string, int> rigTypesIndex = new Dictionary<string, int>();

        //Characters
        public List<CharContainer> charPrefabs = new List<CharContainer>();
        Dictionary<string, int> charIndexes = new Dictionary<string, int>();

        //Weapons
        public List<WeaponContainer> weaponPrefabs = new List<WeaponContainer>();

        //v2
        //this lists are used entirely for categorising, if we want to get a weapon
        //we would still use the dictionary since it's faster
        [HideInInspector]
        public List<WeaponContainer> primary = new List<WeaponContainer>();
        [HideInInspector]
        public List<WeaponContainer> secondary = new List<WeaponContainer>();
        
        //v2
        //We won't need this 2 lists for the purposes of the tutorial
        /*[HideInInspector]
        public List<WeaponContainer> explosives = new List<WeaponContainer>();
        [HideInInspector]
        public List<WeaponContainer> other = new List<WeaponContainer>();*/


        //Items
        public List<ItemContainer> itemPrefabs = new List<ItemContainer>();

        void Awake()
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < charPrefabs.Count; i++)
            {
                if (string.IsNullOrEmpty(charPrefabs[i].rig))
                {
                    Debug.Log("character entry with null id for rig, assigning default");
                    charPrefabs[i].rig = "default";
                }

                if (!rigTypesIndex.ContainsKey(charPrefabs[i].rig))
                {
                    RigType r = new RigType();
                    r.rigName = charPrefabs[i].rig;
                    rigTypes.Add(r);
                    rigTypesIndex.Add(charPrefabs[i].rig, rigTypes.Count - 1);
                }
            }

            for (int i = 0; i < charPrefabs.Count; i++)
            {
                if (charPrefabs[i].charId == null)
                {
                    Debug.Log("character entry with null id, this is not allowed");
                    continue;
                }

                if (!charIndexes.ContainsKey(charPrefabs[i].charId))
                {
                    charIndexes.Add(charPrefabs[i].charId, i);
                }
            }

            for (int i = 0; i < weaponPrefabs.Count; i++)
            {
                if (weaponPrefabs[i].weaponId == null)
                {
                    Debug.Log("weapon entry with null id, this is not allowed");
                    continue;
                }

                if (string.IsNullOrEmpty(weaponPrefabs[i].rig))
                    weaponPrefabs[i].rig = "default";

                RigType rig = GetRigContainer(weaponPrefabs[i].rig);

                if (!rig.weaponIndexes.ContainsKey(weaponPrefabs[i].weaponId))
                {
                    rig.weaponIndexes.Add(weaponPrefabs[i].weaponId, i);
                }

                //v2
                switch (weaponPrefabs[i].cat)
                {
                    case weaponCategory.primary:
                        primary.Add(weaponPrefabs[i]);
                        break;
                    case weaponCategory.secondary:
                        secondary.Add(weaponPrefabs[i]);
                        break;
                    default:
                        break;
                }                
            }

            for (int i = 0; i < itemPrefabs.Count; i++)
            {
                if (itemPrefabs[i].itemId == null)
                {
                    Debug.Log("item entry with null id, this is not allowed");
                    continue;
                }

                if (string.IsNullOrEmpty(itemPrefabs[i].rig))
                    weaponPrefabs[i].rig = "default";

                RigType rig = GetRigContainer(itemPrefabs[i].rig);

                if (!rig.itemIndexes.ContainsKey(itemPrefabs[i].itemId))
                {
                    rig.itemIndexes.Add(itemPrefabs[i].itemId, i);
                }
            }

            if (SessionMaster.singleton == null)
                return;

            if (SessionMaster.singleton.prewarm)
                Prewarm();
        }

        void Prewarm()
        {
            foreach (CharContainer c in charPrefabs)
            {
                GameObject go = Instantiate(c.prefab, -Vector3.one * 5 * 1000, Quaternion.identity) as GameObject;
                c.prefab = go;               

                Rigidbody[] rigs = go.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody r in rigs)
                {
                    r.isKinematic = true;
                }

                go.transform.parent = this.transform;
            }
        }

        public RigType GetRigContainer(string rig)
        {
            if(string.IsNullOrEmpty(rig))
            {
                rig = "default";
                Debug.Log("Rig request with emply string, assigning default");
            }

            RigType r = null;
            int index = -1;

            if (rigTypesIndex.TryGetValue(rig,out index))
            {
                r = rigTypes[index];
            }

            return r;
        }

        public CharContainer GetChar(string charId)
        {
            CharContainer c = null;
            int cIndex = -1;

            if(charIndexes.TryGetValue(charId,out cIndex))
            {
                c = charPrefabs[cIndex];
            }
            else
            {
                Debug.Log(charId + " not found");
            }

            return c;
        }

        public WeaponInstance GetWeapon(string weaponId , string rigName)
        {
            WeaponInstance wi = null;
            int cwcIndex = -1;

            RigType rig = GetRigContainer(rigName);

            if(rig.cwcIndexes.TryGetValue(weaponId,out cwcIndex))
            {
                wi = rig.createdWeaponInstances[cwcIndex];
            }
            else
            {
                WeaponContainer con = null;
                int conIndex = -1;

                if (rig.weaponIndexes.TryGetValue(weaponId, out conIndex))
                {
                    con = weaponPrefabs[conIndex];
                }

                if(con != null)
                {
                    GameObject g = Instantiate(con.prefab);
                    wi = g.GetComponent<WeaponInstance>();
                    rig.createdWeaponInstances.Add(wi);
                    rig.cwcIndexes.Add(weaponId, rig.createdWeaponInstances.Count - 1);
                    g.transform.parent = this.transform;
                    //Debug.Log("created instance for " + weaponId);
                }
            }    

            return wi;
        }

        public ItemInstance GetItem(string itemId, string rigName)
        {
            ItemInstance ii = null;
            int ciInd = -1;

            RigType rig = GetRigContainer(rigName);

            if (rig.ciIndexes.TryGetValue(itemId, out ciInd))
            {
                if (ciInd > rig.createdItems.Count - 1)
                    return null;

                ii = rig.createdItems[ciInd];
            }
            else
            {
                ItemContainer con = null;
                int conIndex = -1;

                if (rig.itemIndexes.TryGetValue(itemId, out conIndex))
                {
                    con = itemPrefabs[conIndex];
                }

                if (con != null)
                {
                    GameObject g = Instantiate(con.prefab);
                    ii = g.GetComponent<ItemInstance>();
                    rig.createdItems.Add(ii);
                    rig.ciIndexes.Add(itemId, rig.createdWeaponInstances.Count - 1);
                    g.transform.parent = this.transform;                  
                }
            }

            return ii;
        }

        public void ResetResources()
        {
            foreach (RigType r in rigTypes)
            {
                r.createdWeaponInstances.Clear();
                r.cwcIndexes.Clear();
                r.createdItems.Clear();
                r.ciIndexes.Clear();
            }
           
            ObjectPool.ObjectPool.singleton.ClearPool();
        }

        static public ResourcesManager singleton;
    }

    public enum weaponCategory
    {
        primary,secondary,explosive,other
    }

    [System.Serializable]
    public class RigType
    {
        public string rigName;
        //Weapons
        public List<WeaponContainer> weapons = new List<WeaponContainer>();
        public Dictionary<string, int> weaponIndexes = new Dictionary<string, int>();
        public List<ItemContainer> items = new List<ItemContainer>();
        public List<WeaponInstance> createdWeaponInstances = new List<WeaponInstance>();
        public Dictionary<string, int> cwcIndexes = new Dictionary<string, int>();

        //Rigs
        public Dictionary<string, int> itemIndexes = new Dictionary<string, int>();
        public List<ItemInstance> createdItems = new List<ItemInstance>();
        public Dictionary<string, int> ciIndexes = new Dictionary<string, int>();
    }

    [System.Serializable]
    public class WeaponContainer
    {
        public string weaponId;
        public string rig;
        public weaponCategory cat;
        public GameObject prefab;
    }

    [System.Serializable]
    public class ItemContainer
    {
        public string itemId;
        public string rig;
        public GameObject prefab;
    }

    [System.Serializable]
    public class CharContainer
    {
        public string charId;
        public string rig;
        public GameObject prefab;
    }
}
