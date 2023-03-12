using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class WeaponMod_CatSelect : MonoBehaviour
    {
        public TPC.weaponCategory category;
        
        public void Press()
        {
            WeaponModScene.singleton.activeCategory = category;
            WeaponModScene.singleton.SetupCategory();
        }
       
    }
}
