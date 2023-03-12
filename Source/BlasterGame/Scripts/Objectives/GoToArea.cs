using UnityEngine;
using System.Collections;

namespace Manager.Objectives
{
    public class GoToArea : ObjectiveReferences
    {
        public Transform UITarget;

        public override void StartBehavior()
        {
            OpenObjects();
            CloseObjects();
            EnableUI();
        }

        public override void FinishBehavior()
        {
            OnFinish();
        }

        void OnFinish()
        {
            //Add more behaviors if needed
            
            DisableUI();
        }

        void EnableUI()
        {
            UI.RadarManager.singleton.AddTrackObj(UITarget.gameObject,Color.blue);
            UI.CanvasOverlay.singleton.goToTarget = UITarget;
            UI.CanvasOverlay.singleton.goToObjective = true;

        }

        void DisableUI()
        {
            UI.RadarManager.singleton.RemoveObj(UITarget.gameObject);
            UI.CanvasOverlay.singleton.goToObjective = false;
        }
    }
}
