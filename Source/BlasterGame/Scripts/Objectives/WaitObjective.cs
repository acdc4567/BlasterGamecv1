using UnityEngine;
using System.Collections;

namespace Manager.Objectives
{
    public class WaitObjective : ObjectiveReferences
    {
        public bool counterIsActive;
        public float waitTime = 5;
        float timer;

        public override void StartBehavior()
        {
            OpenObjects();
            EnableCounter();
        }

        void EnableCounter()
        {
            counterIsActive = true;
        }

        void Update()
        {
            if(counterIsActive)
            {
                timer += Time.deltaTime;
                if(timer > waitTime)
                {
                    LevelObjectives.singleton.FinishObjective();
                    counterIsActive = false;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
