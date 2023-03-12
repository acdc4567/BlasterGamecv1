using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TPC.ObjectPool
{
    public class ObjectPool : MonoBehaviour
    {
        static public ObjectPool singleton;
        Dictionary<string, int> objPoolsDictionary = new Dictionary<string, int>();
        public List<PoolBase> objPools = new List<PoolBase>();

        void Start()
        {
            for (int i = 0; i < objPools.Count; i++)
            {
                if(objPoolsDictionary.ContainsKey(objPools[i].poolName))
                {
                    Debug.Log("Entry with key " + objPools[i].poolName + " is a duplicate!");
                    continue;
                }
                else
                {
                    objPoolsDictionary.Add(objPools[i].poolName, i);
                }
            }
        }

        public GameObject RequestObject(string objName)
        {
            GameObject r = null;
            int index = 0;

            if (objPoolsDictionary.TryGetValue(objName, out index))
            { 
                PoolBase p = objPools[index];

                if (p.createdObjects.Count - 1 < p.budget)
                {
                    r = Instantiate(p.prefab);
                    p.createdObjects.Add(r);
                }
                else
                {
                    p.cur = (p.cur < p.createdObjects.Count - 1) ? p.cur + 1 : 0;
                    r = p.createdObjects[p.cur];
                    r.SetActive(true);
                }
            }
                       
            return r;      
        }

        public void ClearPool()
        {
            for (int i = 0; i < objPools.Count; i++)
            {
                objPools[i].createdObjects.Clear();
                objPools[i].cur = 0;
            }
        }

        void Awake()
        {
            singleton = this;
        }
    }

    [System.Serializable]
    public class PoolBase
    {
        public string poolName;
        public GameObject prefab;
        public int budget = 15;
        public int cur;
        public List<GameObject> createdObjects;
    }
}
