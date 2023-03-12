using UnityEngine;
using System.Collections;

namespace Weapons.Modifications
{
    public class MagazineMod : WeaponMod_Base
    {
        public int magazineAmmo = 30;

        public override void EnableOnWeapon()
        {
            base.Checks();
            getW().activeStats.magazineBullets += magazineAmmo;
        }

        public override void DisableOnWeapon()
        {
            base.Checks();
            getW().activeStats.magazineBullets -= magazineAmmo;
        }
    }
}
