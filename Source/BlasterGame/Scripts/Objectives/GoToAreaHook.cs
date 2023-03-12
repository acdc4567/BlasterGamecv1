using UnityEngine;
using System.Collections;
using TPC;
using Manager;

namespace Manager.Objectives
{
    public class GoToAreaHook : MonoBehaviour
    {

        void OnTriggerEnter(Collider other)
        {
            InputHandler ih = other.GetComponent<InputHandler>();
            if(ih != null)
            {
                LevelObjectives.singleton.FinishObjective();
                gameObject.SetActive(false);
            }
        }
    }
}
