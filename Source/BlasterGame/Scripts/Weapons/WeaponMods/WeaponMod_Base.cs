using UnityEngine;
using System.Collections;
using Weapons;

namespace Weapons.Modifications
{
    public class WeaponMod_Base : MonoBehaviour
    {
        RuntimeWeapon runtime;

        public void SetRuntime(RuntimeWeapon rw)
        {
            runtime = rw;
        }

        public RuntimeWeapon getW()
        {
            return runtime;
        }

        public void Checks()
        {
            if (runtime == null)
                return;
        }

        public virtual void EnableOnWeapon()
        {
         
        }

        public virtual void DisableOnWeapon()
        {
            
        }
    }
}
