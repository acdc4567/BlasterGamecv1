using UnityEngine;
using System.Collections;

namespace Manager
{
    public class ControllersHolder : MonoBehaviour
    {
        public GameObject controllerPrefab;
        [HideInInspector]
        public GameObject controllerInstance;

        public Vector3 startPos;
        public Vector3 startEuler;

        public void CreateCharacter()
        {
            controllerInstance = Instantiate(controllerPrefab) as GameObject;
            UI.RadarManager.singleton.center = controllerInstance.transform;
        }

        public void PlaceCharacter()
        {
            Vector3 targetPos = startPos;
            Vector3 targetEuler = startEuler;

            if(LevelObjectives.singleton.takePositionFromProfile)
            {
                PlayerProfile p = SessionMaster.singleton.GetProfile();

                if (p.progression != 0)
                {
                    targetPos.x = p.px;
                    targetPos.y = p.py;
                    targetPos.z = p.pz;
                    targetEuler.x = p.prx;
                    targetEuler.y = p.pry;
                    targetEuler.z = p.prz;

                    for (int i = 0; i < p.progression; i++)
                    {
                        LevelObjectives.singleton.objectives[i].finished = true;
                    }
                }
            }

            controllerInstance.transform.position = targetPos;
            controllerInstance.transform.eulerAngles = targetEuler;
        }


        static public ControllersHolder singleton;
        void Awake()
        {
            if(singleton == null)
            {
                singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }  
        } 
    }
}
