using UnityEngine;
using System.Collections;
using Manager;

namespace Manager.Objectives
{
    public class FinishLevel : ObjectiveReferences
    {
        public string nextLevel;

        public override void StartBehavior()
        {
           CallToLoadLevel();
        }

        void CallToLoadLevel()
        {
            SessionMaster.singleton.LoadLevel(nextLevel);
        }
    }
}
