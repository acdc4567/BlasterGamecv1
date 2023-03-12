using UnityEngine;
using System.Collections;
using Weapons;
using Weapons.Modifications;

namespace TPC
{
    public static class Statics
    {

        #region hash
        public static string horizontal = "horizontal";
        public static string vertical = "vertical";
        public static string special = "special";
        public static string specialType = "specialType";
        public static string onLocomotion = "onLocomotion";
        public static string Horizontal = "Horizontal";
        public static string Vertical = "Vertical";
        public static string jumpType = "jumpType";
        public static string Jump = "Jump";
        public static string onAir = "onAir";
        public static string mirrorJump = "mirrorJump";
        public static string incline = "incline";
        public static string shootInput = "Shoot";
        public static string aimInput = "Aim";
        public static string runInput = "Run";
        public static string inSpecial = "inSpecial";
        public static string walkVault = "vault_over_walk_1";
        public static string runVault = "vault_over_run";
        public static string walk_up = "walk_up";
        public static string run_up = "run_up";
        public static string onSprint = "onSprint";
        public static string climb_up = "climb_up_high";
        public static string climb_up_medium = "climb_up_medium";
        public static string aim = "aim";
        public static string normal = "normal";
        public static string weaponType = "weaponType";
        public static string shoot = "shoot";
        public static string turn = "turn";
        public static string rifle_switch = "rifle_switch";
        public static string pistol_switch = "pistol_switch";
        public static string closeIK = "closeIK";
        public static string closeHeadIK = "closeHeadIK";
        public static string rifle_reload = "rifle_reload";
        public static string pistol_reload = "pistol_reload";
        public static string inIdle = "inIdle";
        public static string reloadInput = "Reload";
        public static string firstPersonInput = "FirstPerson";
        public static string switchInput = "Switch";
        public static string pivotInput = "Pivot";
        public static string vaultInput = "Vault";
        public static string coverInput = "Cover";
        public static string coverPeak = "CoverPeak";
        public static string crouchInput = "Crouch";
        public static string crouch_anim = "crouch";
        public static string death = "death";
        public static string damage_particle = "damage_particle";
        #endregion

        #region Coroutine Names
        public static string DelaySwitch = "DelaySwitch";
        public static float delay_switch_time = 1;
        #endregion

        #region Variables
        public static float vaultCheckDistance = 2;
        public static float vaultCheckDistance_Run = 2.5f;
        public static float vaultSpeedWalking = 2;
        public static float walkUpSpeed = 1.8f;
        public static float vaultSpeedRunning = 4.5f;
        public static float vaultSpeedIdle = 1;
        public static float climbMaxHeight = 2.2f;
        public static float walkUpHeight = 1;
        public static float walkUpThreshold = 0.4f;
        public static float climbSpeed = .5f;
        public static float climbUpStartPosOffset = 0.5f;
        public static float aimHelperSpeed = 18;
        public static float lookForCoverDistance = 2;
        //Weapons
        public static float shoulderRotateSpeed = 5;

        #endregion

        #region Functions
        public static int GetAnimSpecialType(AnimSpecials i)
        {
            int r = 0;
            switch (i)
            {
                case AnimSpecials.runToStop:
                    r = 11;
                    break;
                case AnimSpecials.run:
                    r = 10;
                    break;
                case AnimSpecials.jump_idle:
                    r = 21;
                    break;
                case AnimSpecials.run_jump:
                    r = 22;
                    break;
                case AnimSpecials.vault_over_walk_1:
                    r = 33;
                    break;
                default:
                    break;
            }

            return r;
        }

        public static void CopyWeaponStatsFromWeaponInstance(ref WeaponStats to, WeaponStats from)
        {
            to.mainHand_pos = from.mainHand_pos;
            to.mainHand_rot = from.mainHand_rot;
            to.offHand_pos_aim = from.offHand_pos_aim;
            to.offHand_rot_aim = from.offHand_rot_aim;
            to.offHand_pos_idle = from.offHand_pos_idle;
            to.offHand_rot_idle = from.offHand_rot_idle;
            to.useHeadTarget = from.useHeadTarget;
            to.headTargetPos = from.headTargetPos;
            to.bulletSpawnPosition = from.bulletSpawnPosition;
            to.aimguide_offset = from.aimguide_offset;
            to.recoilY = from.recoilY;
            to.recoilZ = from.recoilZ;
            to.angularX = from.angularX;
            to.offsetYscale = from.offsetYscale;
            to.offsetZscale = from.offsetZscale;
            to.fireRate = from.fireRate;
            to.cameraRecoilX = from.cameraRecoilX;
            to.cameraRecoilY = from.cameraRecoilY;
        }

        public static void CopyItemFromTo(ref Item to, Item from)
        {
            to.itemId = from.itemId;
            to.bone = from.bone;
            to.localPosition = from.localPosition;
            to.localEuler = from.localEuler;
            to.localScale = from.localScale;
            to.modelPrefab = from.modelPrefab;
        }

        //v2
        public static void CopyModFromTo(ref WeaponMod_Vis to, WeaponMod_Vis from)
        {
            to.modId = from.modId;
            to.localEuler = from.localEuler;
            to.localPosition = from.localPosition;
            to.modType = from.modType;
            to.visPrefab = from.visPrefab;
            to.localScale = from.localScale;
        }
        #endregion


        public static float RandomFloat(float min, float max)
        {
            return Random.Range(min, max);
        }
    }


    public enum AnimSpecials
    {
        run, runToStop, jump_idle, run_jump, vault_over_walk_1
    }
}
