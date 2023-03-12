using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    [System.Serializable]
    public class WeaponMod_Vis
    {
        public string modId;
        public WModType modType;
        public Vector3 localPosition;
        public Vector3 localEuler;
        public Vector3 localScale;
        public GameObject visPrefab;
    }
}
