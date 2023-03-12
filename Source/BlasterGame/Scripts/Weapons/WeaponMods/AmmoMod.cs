using UnityEngine;
using System.Collections;

namespace Weapons.Modifications
{
    public class AmmoMod : WeaponMod_Base
    {
        public int maxAmmo = 30;

        public override void EnableOnWeapon()
        {
            base.Checks();
            getW().activeStats.maxCarryBullets += maxAmmo;
        }

        public override void DisableOnWeapon()
        {
            base.Checks();
            getW().activeStats.maxCarryBullets = getW().wReference.weaponStats.maxCarryBullets;
        }
    }
}