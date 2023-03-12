using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WeaponStats))]
public class WeaponReferenceBase : MonoBehaviour {

    public string weaponID;
    public GameObject weaponModel;
    public Animator modelAnimator;
    public GameObject ikHolder;
    public Animator weaponAnim;
    public Transform rightHandTarget;
    public Transform rightHandRotation;
    public Transform leftHandTarget;
    public Transform lookTarget;
    public ParticleSystem[] muzzle;
    public Transform bulletSpawner;
    public Transform casingSpawner;
        
    public Transform rightElbowTarget;
    public Transform leftElbowTarget;
    public int animType;

    public bool dis_LHIK_notAiming;

    public int carryingAmmo = 60;
    public int maxAmmo = 60;
    public GameObject pickablePrefab;

    public GameObject holsterWeapon;
    public bool meleeWeapon = false;

    public bool akimbo = false;
    public bool dontShoot;
    public bool emptyGun;
    public bool forReload;
    public Vector3 aimPosition;
    public WeaponStats weaponStats;

    //dis
    public bool onRunDisableIK;
    public WeaponType weaponType;
}

public enum WeaponType
{
    normal,
    melee,
    akimbo,
    bow
}