using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Manager.Objectives
{
    public class KillTargets : ObjectiveReferences
    {
        public int targetsDown;

        public List<GameObject> objsToTrack = new List<GameObject>();

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
            foreach (GameObject go in objectsToOpen)
            {
                go.SetActive(false);
            }

            DisableUI();
        }

        void EnableUI()
        {
            UI.CanvasOverlay.singleton.goToIndicator.gameObject.SetActive(false);
            UI.CanvasOverlay.singleton.goToObjective = false;

            UI.RadarManager rad = UI.RadarManager.singleton;

            for (int i = 0; i < objsToTrack.Count; i++)
            {
                rad.AddTrackObj(objsToTrack[i], Color.red);
            }
        }

        void DisableUI()
        {
            UI.RadarManager rad = UI.RadarManager.singleton;

            for (int i = 0; i < objsToTrack.Count; i++)
            {
                rad.RemoveObj(objsToTrack[i]);
            }

            UI.CanvasOverlay.singleton.goToObjective = false;
        }

        public void CheckProgress()
        {
            if(objsToTrack.Count == 0)
            {
                LevelObjectives.singleton.FinishObjective();
            }
        }
    }
}

