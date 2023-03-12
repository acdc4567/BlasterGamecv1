using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponMod_NoneButton : MonoBehaviour
    {
        public WModType type;

        public void Press()
        {
            WeaponModScene.singleton.RemoveMod(type);
        }
    }
}
