using UnityEngine;
using System.Collections;
using Manager;

namespace Other
{
    public class LevelTarget : MonoBehaviour
    {
        int timesHit;

        public Manager.Objectives.KillTargets hook;
        
        public void Hit()
        {
            timesHit++;

            if(timesHit > 3)
            {
                gameObject.SetActive(false);

                if (hook.objsToTrack.Contains(this.gameObject))
                    hook.objsToTrack.Remove(this.gameObject);

                hook.CheckProgress();

                UI.RadarManager.singleton.RemoveObj(this.gameObject);
            }
        }

    }
}