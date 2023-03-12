using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Modifications
{
    public class OverwriteIKMod : WeaponMod_Base
    {
        public Transform ikTarget;

        public override void EnableOnWeapon()
        {
            base.Checks();

            if (ikTarget == null)
                return;

            Vector3 localPosition = ikTarget.localPosition;
            Vector3 localEuler = ikTarget.localEulerAngles;
            ikTarget.transform.parent = getW().modelReferences.boneHelper.ReturnHelper(HumanBodyBones.RightHand).helper;
            ikTarget.localPosition = localPosition;
            ikTarget.localEulerAngles = localEuler;
            getW().activeStats.offHand_pos_idle = ikTarget.localPosition;
            getW().activeStats.offHand_rot_idle = ikTarget.localEulerAngles;
        }

        public override void DisableOnWeapon()
        {
            base.Checks();
            ikTarget.parent = this.transform;
            getW().activeStats.offHand_pos_idle = getW().wReference.weaponStats.offHand_pos_idle;
            getW().activeStats.offHand_rot_idle = getW().wReference.weaponStats.offHand_rot_idle;
        }

    }
}
