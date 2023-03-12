using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Weapons.Modifications
{
    public class WeaponMod_WeaponSelect : MonoBehaviour
    {
        public Text weaponName;
        public Image weaponIcon;
        public string weaponId;

        public void Init(string id)
        {
            weaponId = id;
            weaponName.text = weaponId.ToUpper();
        }

        public void Press()
        {
            WeaponModScene.singleton.RequestWeapon(weaponId);
        }
    }
}
