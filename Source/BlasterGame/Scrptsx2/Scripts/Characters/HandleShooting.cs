using UnityEngine;
using System.Collections;

public class HandleShooting : MonoBehaviour
{

    StateManager states;

    public GameObject smokeParticle;
    public GameObject casingPrefab;

    WeaponManager weaponManager;


    public void Init()
    {
        states = GetComponent<StateManager>();
        weaponManager = GetComponent<WeaponManager>();
    }

    public void Tick()
    {
        WeaponReferenceBase curWeapon = weaponManager.ReturnCurrentWeapon();

        switch (curWeapon.weaponType)
        {
            case WeaponType.normal:
                Shooting(curWeapon);
                break;
            case WeaponType.melee:
                Melee(curWeapon);
                break;
            case WeaponType.akimbo:
                Shooting(curWeapon);
                break;
            case WeaponType.bow:
                HandleBow(curWeapon);
                break;
            default:
                break;
        }
    }


    public void HandleModelAnimator(WeaponReferenceBase weapon)
    {
        if (weapon.modelAnimator != null)
        {
            weapon.modelAnimator.SetBool("Shoot", false);

            if (weapon.weaponStats.curBullets > 0)
            {
                weapon.modelAnimator.SetBool("Empty", false);
            }
            else
            {
                weapon.modelAnimator.SetBool("Empty", true);
            }
        }
    }

    public void Shooting(WeaponReferenceBase weapon)
    {
        HandleModelAnimator(weapon);

        if (states.shoot)
        {
            ActualShooting(weapon);
        }
    }

    public void ActualShooting(WeaponReferenceBase weapon)
    {
        if (weapon.weaponAnim == null)
            weapon.weaponAnim = weapon.ikHolder.GetComponent<Animator>();

        if (!weapon.dontShoot)
        {
            if (weapon.modelAnimator != null)
            {
                weapon.modelAnimator.SetBool("Shoot", false);
            }

            weapon.weaponAnim.SetBool("Shoot", false);

            if (weapon.weaponStats.curBullets > 0)
            {
                weapon.emptyGun = false;

                if (weapon == weaponManager.ReturnCurrentWeapon())
                    states.audioManager.PlayGunSound();
                else
                    GetComponent<DualWield.Akimbo>().akimboAudioSource.Play();


                if (weapon.modelAnimator != null)
                {
                    weapon.modelAnimator.SetBool("Shoot", true);
                }

                weapon.weaponAnim.SetBool("Shoot", true); //moved the ik animation here

                GameObject go = Instantiate(casingPrefab, weapon.casingSpawner.position, weapon.casingSpawner.rotation) as GameObject;
                Rigidbody rig = go.GetComponent<Rigidbody>();
                rig.AddForce(transform.right.normalized * 2 + Vector3.up * 1.3f, ForceMode.Impulse);
                rig.AddRelativeTorque(go.transform.right * 1.5f, ForceMode.Impulse);

                for (int i = 0; i < weapon.muzzle.Length; i++)
                {
                    weapon.muzzle[i].Emit(1);
                }

                weapon.dontShoot = true;
                StartCoroutine(HandleFireRate(weapon));
                StartCoroutine(HandleWeaponAnim(weapon));
                RaycastShoot(weapon);
                states.actualShooting = true;

                weapon.weaponStats.curBullets = weapon.weaponStats.curBullets - 1;
            }
            else
            {
                if (weapon.emptyGun)
                {
                    if (weapon == weaponManager.ReturnCurrentWeapon())
                    {
                        ReloadWeapon(weapon);

                        if (weapon.akimbo)
                            ReloadWeapon(GetComponent<DualWield.Akimbo>().akimboWeapon);
                    }
                }
                else
                {
                    states.audioManager.PlayEffect("empty_gun");
                    weapon.emptyGun = true;
                }
            }
        }
        else
        {
            weapon.weaponAnim.SetBool("Shoot", false);
        }
    }

    IEnumerator HandleWeaponAnim(WeaponReferenceBase weapon)
    {
        yield return new WaitForEndOfFrame();
        if (weapon.weaponAnim)
            weapon.weaponAnim.SetBool("Shoot", false);
    }

    IEnumerator HandleFireRate(WeaponReferenceBase weapon)
    {
        yield return new WaitForSeconds(weapon.weaponStats.fireRate);
        weapon.dontShoot = false;
    }

    public void ReloadWeapon(WeaponReferenceBase weapon)
    {
        if (weapon.carryingAmmo > 0 && !states.down)
        {
            if (!weapon.forReload)
            {
                if (weapon == weaponManager.ReturnCurrentWeapon())
                    states.handleAnim.StartReload();

                StartCoroutine(FillBullets(weapon));
                weapon.forReload = true;
            }
        }
        else
        {
            states.audioManager.PlayEffect("empty_gun");
        }
    }

    IEnumerator FillBullets(WeaponReferenceBase weapon)
    {
        yield return new WaitForSeconds(0.2f);
        int targetBullets = 0;

        if (weapon.weaponStats.maxBullets < weapon.carryingAmmo)
        {
            targetBullets = weapon.weaponStats.maxBullets;
        }
        else
        {
            targetBullets = weapon.carryingAmmo;
        }

        weapon.carryingAmmo -= targetBullets;

        weapon.weaponStats.curBullets = targetBullets;

        weapon.forReload = false;
    }

    void RaycastShoot(WeaponReferenceBase weapon)
    {
        if (weapon == weaponManager.ReturnCurrentWeapon())
            weapon.aimPosition = states.lookHitPosition;

        Vector3 direction = weapon.aimPosition - weapon.bulletSpawner.position;
        RaycastHit hit;

        if (Physics.Raycast(weapon.bulletSpawner.position, direction, out hit, 100, states.layerMask))
        {
            GameObject go = Instantiate(smokeParticle, hit.point, Quaternion.identity) as GameObject;
            go.transform.LookAt(weapon.bulletSpawner.position);

            if (hit.transform.GetComponent<ShootingRangeTarget>())
            {
                hit.transform.GetComponent<ShootingRangeTarget>().HitTarget();
            }
        }
    }

    public void Melee(WeaponReferenceBase weapon)
    {
        if (states.shoot)
        {
            if (!weapon.dontShoot)
            {
                weapon.dontShoot = true;

                states.handleAnim.anim.CrossFade("Knife Hit", 0.5f);

                StartCoroutine(delayedAttack(weapon));
                StartCoroutine(HandleFireRate(weapon));
            }
        }
    }

    IEnumerator delayedAttack(WeaponReferenceBase weapon)
    {
        yield return new WaitForSeconds(0.8f);
        if (weapon == weaponManager.ReturnCurrentWeapon())
            weapon.aimPosition = states.lookHitPosition;

        Vector3 dir = weapon.aimPosition - weapon.bulletSpawner.position;

        RaycastHit hit;

        if (Physics.Raycast(weapon.bulletSpawner.position, dir.normalized, out hit, 2, states.layerMask))
        {
            if (hit.transform.GetComponent<ShootingRangeTarget>())
            {
                hit.transform.GetComponent<ShootingRangeTarget>().HitTarget();
            }
        }
    }

    BowLogic bowL;

    void HandleBow(WeaponReferenceBase weapon)
    {
        if (weapon == weaponManager.ReturnCurrentWeapon())
            weapon.aimPosition = states.lookHitPosition;

        if (bowL == null)
        {
            bowL = weapon.transform.GetComponent<BowLogic>();
        }

        states.handleAnim.anim.SetBool("Akimbo", true);

        if (states.aiming)
        {
            if (!weapon.dontShoot && weapon.carryingAmmo != 0)
            {
                bowL.holsterArrow.SetActive(true);

                if (states.shoot)
                {
                    if (weapon.weaponStats.curBullets > 0)
                    {
                        weapon.emptyGun = false;

                        Vector3 dir = weapon.aimPosition - weapon.rightHandTarget.position;
                        dir.Normalize();
                        Quaternion targetRot = Quaternion.LookRotation(dir);

                        Vector3 spawnP = weapon.rightHandTarget.position;
                        spawnP.y += 0.05f;

                        GameObject go = Instantiate(
                            bowL.arrowPrefab, spawnP, targetRot) as GameObject;

                        go.GetComponent<ArrowLogic>().owner = states;

                        weapon.dontShoot = true;
                        StartCoroutine(HandleFireRate(weapon));
                        weapon.weaponStats.curBullets = weapon.weaponStats.curBullets - 1;

                        if (weapon.carryingAmmo > 0)
                            ReloadWeapon(weapon);
                    }

                }
            }
            else
            {
                bowL.holsterArrow.SetActive(false);
            }
        }
        else
        {
            bowL.holsterArrow.SetActive(false);
        }
    }
}
