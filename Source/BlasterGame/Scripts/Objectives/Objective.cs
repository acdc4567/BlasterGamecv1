using UnityEngine;
using System.Collections;

namespace Manager.Objectives
{
    [System.Serializable]
    public class Objective 
    {
        public string objectiveDescription;
        public bool finished;
        public bool isCheckpoint;
        public bool isFalseObjective;
        public ObjectiveReferences references;
    }
}
