using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Manager.Objectives;
using UI;

namespace Manager
{
    public class LevelObjectives : MonoBehaviour
    {
        public int curObjective;
        public List<Objective> objectives = new List<Objective>();

        CanvasOverlay cOverlay;

        public bool takePositionFromProfile;

        void Start()
        {
            if (SessionMaster.singleton.debugMode)
            {
                gameObject.SetActive(false);
                return;
            }

            cOverlay = CanvasOverlay.singleton;
            StartCoroutine("StartFirstObjective");
        }

        IEnumerator StartFirstObjective()
        {
            yield return new WaitForEndOfFrame();
            objectives[0].references.StartObjective();
        }

        public void FinishObjective()
        {
            objectives[curObjective].finished = true;
            objectives[curObjective].references.FinishObjective();
                      
            if (objectives[curObjective].isCheckpoint)
            {
                PlayerProfile p = SessionMaster.singleton.GetProfile();
                Transform cont = ControllersHolder.singleton.controllerInstance.transform;
                Vector3 targetPos = cont.position;
                Vector3 targetEuler = cont.eulerAngles;

                p.px = targetPos.x;
                p.py = targetPos.y;
                p.pz = targetPos.z;
                p.prx = targetEuler.x;
                p.pry = targetEuler.y;
                p.prz = targetEuler.z;
                p.progression = curObjective;
                SessionMaster.singleton.SaveProfile();
            }

            if (curObjective < objectives.Count -1)
            {
                curObjective++;
                objectives[curObjective].references.StartObjective();

                bool falseObj = false;
                
                if(curObjective - 1 >= 0)
                {
                    Objective prev = objectives[curObjective-1];
                    falseObj = prev.isFalseObjective;
                }

                cOverlay.OpenObjective(objectives[curObjective].objectiveDescription, !falseObj);
             
            }
            else
            {
                //Finished Level callback
            }
        }

        static public LevelObjectives singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
