using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class SilencerMod : WeaponMod_Base
    {

        public override void EnableOnWeapon()
        {
            for (int i = 0; i < getW().modelReferences.otherParticles.Length; i++)
            {
                getW().modelReferences.otherParticles[i].gameObject.SetActive(false);
            }
           
        }

        public override void DisableOnWeapon()
        {
            for (int i = 0; i < getW().modelReferences.otherParticles.Length; i++)
            {
                getW().modelReferences.otherParticles[i].gameObject.SetActive(true);
            }
        }
    }
}
