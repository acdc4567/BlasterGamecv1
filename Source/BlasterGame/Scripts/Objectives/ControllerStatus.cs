using UnityEngine;
using System.Collections;

namespace Manager.Objectives
{
    public class ControllerStatus : ObjectiveReferences
    {
        public bool status;
        public Transform targetPosition;

        ControllersHolder ctrH;

        void Start()
        {   
            ctrH = ControllersHolder.singleton;
        }

        public override void StartBehavior()
        {
            ControllerUpdateStatus();
        }

        void ControllerUpdateStatus()
        {
            if (ctrH.controllerInstance == null)
                ctrH.CreateCharacter();

            if (targetPosition)
            { ctrH.startPos = targetPosition.position;
                ctrH.startEuler = targetPosition.eulerAngles;
            }
            else
            { ctrH.startPos = Vector3.zero;
                ctrH.startEuler = Vector3.zero;
            }

            ctrH.PlaceCharacter();
            ctrH.controllerInstance.SetActive(status);

            LevelObjectives.singleton.FinishObjective();
        }
        
    }
}
