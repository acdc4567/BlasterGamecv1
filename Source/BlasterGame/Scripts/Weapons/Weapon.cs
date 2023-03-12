using UnityEngine;
using System.Collections.Generic;

namespace Weapons {
    [System.Serializable]
    public class Weapon {

        public string weaponId;
        public GameObject modelPrefab;
        public bool hasHolster;
        public HumanBodyBones holsterBone = HumanBodyBones.RightUpperLeg;      
        public GameObject holsterPrefab;

        //v2
        [Header("For weapon Mods")]//although you can pretty much set it for animations too
        public TPC.weaponCategory weaponCategory;


        [Header("0: Rifles 1: Pistols")]
        public int weaponAnimSet = 0;
        [Header("Pickable prefab id")]
        public string pickId;
        public WeaponStats weaponStats; 

        [HideInInspector]
        public Vector3 modelPos;
        [HideInInspector]
        public Vector3 modelRot;
        [HideInInspector]
        public Vector3 modelScale;

        [HideInInspector]
        public Vector3 holsterPos;
        [HideInInspector]
        public Vector3 holsterRot;
        [HideInInspector]
        public Vector3 holsterScale;

        public Weapon()
        {
            weaponStats = new WeaponStats();
        }
    }

    [System.Serializable]
    public class WeaponStats
    {
        [Header("Bullets etc.")]
        public int magazineBullets = 30;
        public int curCarryBullets = 120;
        public int maxCarryBullets = 120;
        public bool hasInfiniteAmmo;

        [Header("Camera Handling")]
        public float turnSpeed = 0.5f;
        public float turnSpeedController = 3.5f;
        public Vector3 fps_camera_offset = new Vector3(0,0.015f,0);

        [Header("Weapon Behavior")]
        public float fireRate = 0.2f;
        public float cameraRecoilY = 0.5f;
        public float cameraRecoilX = 0;
        [HideInInspector]
        public float frate;
           
        [Header("Aim pivot offset")]
        public Vector3 shoulder_offset;
        [Header("Offsets")]
        public float offsetYscale = 0.1f;
        public float offsetZscale = 0.1f;
        [Header("Recoil")]
        public AnimationCurve recoilY;
        public AnimationCurve recoilZ;
        public AnimationCurve angularX;

        [Header("IK References")]
        public bool useHeadTarget = true;
        [HideInInspector]
        public Vector3 headTargetPos;
        [HideInInspector]
        public Vector3 mainHand_pos;
        [HideInInspector]
        public Vector3 mainHand_rot;
        [HideInInspector]
        public Vector3 offHand_pos_aim;
        [HideInInspector]
        public Vector3 offHand_rot_aim;
        [HideInInspector]
        public Vector3 offHand_pos_idle;
        [HideInInspector]
        public Vector3 offHand_rot_idle;
        [HideInInspector]
        public Vector3 aimguide_offset;

        public Vector3 bulletSpawnPosition;
        
        //V2
        public Weapons.Modifications.WeaponMod_Hook modificationsHook;

        public void LoadBullets(int value)
        {
            curCarryBullets += value;
            if (curCarryBullets > maxCarryBullets)
                curCarryBullets = maxCarryBullets;
        }
    }
}
