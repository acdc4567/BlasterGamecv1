using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace UI
{
    public class RadarManager : MonoBehaviour
    {
        public Transform center;
        float scale = 5;
        public Transform radar;

        GameObject pointPrefab;

        List<RadarObj> tracking = new List<RadarObj>();
        List<RadarObj> toUnregister = new List<RadarObj>();

        void Start()
        {
            pointPrefab = Resources.Load("radarPrefab") as GameObject;
        }

        void Update()
        {
            if (center == null)
                return;

            foreach (RadarObj o in tracking)
            {
                Vector3 relativePos = o.wObj.transform.position - center.position;
                Vector2 uiPos = Vector2.zero;
                uiPos.x = relativePos.x;
                uiPos.y = relativePos.z;
                uiPos *= scale;
                uiPos.x = Mathf.Clamp(uiPos.x ,-45 , 45);
                uiPos.y = Mathf.Clamp(uiPos.y ,-45, 45);
                o.icon.transform.localPosition = uiPos;
            }

            if (toUnregister.Count > 0)
            {
                foreach (RadarObj o in toUnregister)
                {
                    if (tracking.Contains(o))
                        tracking.Remove(o);

                    Destroy(o.icon);
                }

                toUnregister.Clear();
            }
        }

        public void AddTrackObj(GameObject obj, Color clr)
        {
            RadarObj r = new RadarObj();
            r.wObj = obj;
            GameObject u = Instantiate(pointPrefab) as GameObject;
            u.transform.SetParent(radar);
            r.icon = u;
            r.icon.GetComponent<Image>().color = clr;
            tracking.Add(r);
        }

        public void RemoveObj(GameObject go)
        {
            for (int i = 0; i < tracking.Count; i++)
            {
                if (tracking[i].wObj == go)
                {
                    toUnregister.Add(tracking[i]);
                    break;
                }
            }
        }

        static public RadarManager singleton;
        void Awake()
        {
            singleton = this;
        }
    }

    [System.Serializable]
    public class RadarObj
    {
        public GameObject wObj;
        public GameObject icon;
    }
}
