using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponMod_Hook : MonoBehaviour
    {
        public string[] availableMods;
    }

    [System.Serializable]
    public class WM_Place
    {
        public Transform place;
        public WModType type;
        public string modId;
    }
}
