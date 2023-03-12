using UnityEngine;
using System.Collections;

namespace Manager.Objectives
{
    public class ObjectiveReferences : MonoBehaviour
    {
        public GameObject[] objectsToOpen;
        public GameObject[] objectsToClose;

        public void StartObjective()
        {
            StartBehavior();
        }

        public void FinishObjective()
        {
            FinishBehavior();
        }

        public virtual void StartBehavior()
        {

        }

        public virtual void FinishBehavior()
        {

        }

        public void OpenObjects()
        {
            foreach (GameObject go in objectsToOpen)
            {
                go.SetActive(true);
            }
        }

        public void CloseObjects()
        {
            foreach (GameObject go in objectsToClose)
            {
                go.SetActive(false);
            }
        }
    }
}
