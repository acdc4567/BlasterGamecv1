using UnityEngine;
using System.Collections;
using System;
using TPC;
using Weapons;
using Other;
using TPC.ObjectPool;

namespace Manager {
    public static class Ballistics
    {

        public static void RayBullets(Vector3 origin, Vector3 direction, RuntimeWeapon rw)
        {
            RaycastHit hit;
            Debug.DrawRay(origin, direction);
            LayerMask layerMask = ~(1 << 2 | 1 << 8);
            if (Physics.Raycast(origin, direction, out hit, 100, layerMask))
            {
                CheckHit(hit,rw);

                GameObject particle = ObjectPool.singleton.RequestObject(Statics.damage_particle);
                if (particle)
                {
                    particle.transform.position = hit.point;
                    particle.transform.LookAt(origin);
                }
            }
        }

        static void CheckHit(RaycastHit hit, RuntimeWeapon rw)
        {
            StateManager st = hit.transform.GetComponentInParent<StateManager>();

            if (st != null)
            {
                Debug.Log("hit character");
            }

            LevelTarget lt = hit.transform.GetComponentInParent<LevelTarget>();
            if(lt != null)
            {
                lt.Hit();
            }
        }
    }
}
