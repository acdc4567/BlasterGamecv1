using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour {

    public int maxWeapons = 1;
    public List<WeaponReferenceBase> AvailableWeapons = new List<WeaponReferenceBase>();

    public int weaponIndex;
    [HideInInspector]
    public List<WeaponReferenceBase> Weapons = new List<WeaponReferenceBase>();
    WeaponReferenceBase currentWeapon;
    IKHandler ikHandler;
    [HideInInspector]
    public StateManager states;
    CharacterAudioManager audioManager;

    public WeaponReferenceBase unarmed;

    public bool startUnarmed;

    public List<ShareableObj> additionalShareableObj = new List<ShareableObj>();

    public void Init()
    {
        states = GetComponent<StateManager>();
        ikHandler = GetComponent<IKHandler>();
        audioManager = GetComponent<CharacterAudioManager>();

        WeaponReferenceBase[] allWeapons = GetComponentsInChildren<WeaponReferenceBase>();
        Weapons.Clear();
        Weapons.AddRange(allWeapons);

        CloseAllWeapons();

        unarmed = CreateUnarmed();
        unarmed.animType = 10;
        unarmed.weaponID = "unarmed";
        unarmed.meleeWeapon = true;

        if (startUnarmed)
        {
            SwitchWeaponWithTargetWeapon(unarmed);
        }
        else
        {
            if (AvailableWeapons.Count == 0)
                AvailableWeapons.Add(Weapons[weaponIndex]);

            SwitchWeapon(weaponIndex);
        }

    }

    WeaponReferenceBase CreateUnarmed()
    {
        GameObject go = new GameObject();
        go.AddComponent<WeaponReferenceBase>();
        go.name = "unarmed weapon";
        go.transform.parent = transform.parent;
        return go.GetComponent<WeaponReferenceBase>();
    }

    public void Tick()
    {
        //test switch
        if(Input.GetKeyUp(KeyCode.Q))
        {
            SwitchWeapon(weaponIndex);

            if (weaponIndex < AvailableWeapons.Count - 1)
            {
                weaponIndex++;
            }
            else
            {
                weaponIndex = 0;
            }
        }

        //test unarmed
        if(Input.GetKeyDown(KeyCode.H))
        {
            if (!states.meleeWeapon)
            {
                SwitchWeaponWithTargetWeapon(unarmed);      
            }
            else
            {
                SwitchWeaponWithTargetWeapon(AvailableWeapons[weaponIndex]);
            }
        }
    }

    public WeaponReferenceBase ReturnWeaponWithID(string weaponID)
    {
        WeaponReferenceBase retVal = null;

        for (int i = 0; i < Weapons.Count; i++)
        {
            if(string.Equals(Weapons[i].weaponID, weaponID))
            {
                retVal = Weapons[i];
                break;
            }
        }

        return retVal;
    }

    public WeaponReferenceBase ReturnCurrentWeapon()
    {
        return currentWeapon;
    }


    public void SwitchWeapon(int desiredIndex)
    {
        if (desiredIndex > AvailableWeapons.Count - 1)
        {
            desiredIndex = 0;
            weaponIndex = 0;
        }

        WeaponReferenceBase targetWeapon = ReturnWeaponWithID(AvailableWeapons[desiredIndex].weaponID);

        SwitchWeaponWithTargetWeapon(targetWeapon);

        weaponIndex = desiredIndex;
    }

    public void SwitchWeaponWithTargetWeapon(WeaponReferenceBase targetWeapon)
    {   
        if (currentWeapon != null)
        {
            if (currentWeapon.weaponModel != null)
            {
                currentWeapon.weaponModel.SetActive(false);
                
                if(currentWeapon.ikHolder)
                    currentWeapon.ikHolder.SetActive(false);
            }

            if (currentWeapon.holsterWeapon)
                currentWeapon.holsterWeapon.SetActive(true);
        }

        WeaponReferenceBase newWeapon = targetWeapon;

        if (newWeapon.weaponStats == null)
            newWeapon.weaponStats = newWeapon.transform.GetComponent<WeaponStats>();

        if(newWeapon.weaponStats == null)
        {
            newWeapon.gameObject.AddComponent<WeaponStats>();
            newWeapon.weaponStats = newWeapon.transform.GetComponent<WeaponStats>();
        }

        if (newWeapon.holsterWeapon)
        { newWeapon.holsterWeapon.SetActive(false); }

        if (newWeapon.rightHandTarget)
        { ikHandler.rightHandIkTarget = newWeapon.rightHandTarget; }
        else
        { ikHandler.rightHandIkTarget = null; }

        if (newWeapon.rightHandRotation)
        { ikHandler.rightHandIKRotation = newWeapon.rightHandRotation; }
        else
        { ikHandler.rightHandIKRotation = null; }

        if (newWeapon.leftHandTarget)
        { ikHandler.leftHandIkTarget = newWeapon.leftHandTarget; }
        else
        { ikHandler.leftHandIkTarget = null; }
        
        if (newWeapon.lookTarget)
        { ikHandler.overrideLookTarget = newWeapon.lookTarget; }
        else
        { ikHandler.overrideLookTarget = null; }

        if (newWeapon.leftElbowTarget) { ikHandler.leftElbowTarget = newWeapon.leftElbowTarget; }
        else { ikHandler.leftElbowTarget = null; }

        if (newWeapon.rightElbowTarget) { ikHandler.rightElbowTarget = newWeapon.rightElbowTarget; }
        else { ikHandler.rightElbowTarget = null; }

        ikHandler.LHIK_dis_notAiming = newWeapon.dis_LHIK_notAiming;

        if (newWeapon.dis_LHIK_notAiming)
            ikHandler.leftHandIKweight = 0;

        audioManager.gunSounds.clip = newWeapon.weaponStats.shootSound;

        if(newWeapon.weaponModel)
            newWeapon.weaponModel.SetActive(true);
        
        if(newWeapon.ikHolder)
            newWeapon.ikHolder.SetActive(true);

        states.weaponAnimType = newWeapon.animType;
        states.meleeWeapon = newWeapon.meleeWeapon;

        states.ikHandler.AkimboStatus(newWeapon.akimbo);

        currentWeapon = newWeapon;
    }

    void CloseAllWeapons()
    { //close and init
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].weaponModel)
            {
                if (!Weapons[i].meleeWeapon)
                {
                    ParticleSystem[] muzzleParticles = Weapons[i].weaponModel.GetComponentsInChildren<ParticleSystem>();
                    Weapons[i].muzzle = muzzleParticles;

                    Weapons[i].weaponModel.SetActive(false);
                    Weapons[i].ikHolder.SetActive(false);

                    if (Weapons[i].holsterWeapon)
                        Weapons[i].holsterWeapon.SetActive(false);
                }
            }
        }

        ShareableObj[] sh = GetComponentsInChildren<ShareableObj>();
        foreach (ShareableObj o in sh)
        {
            o.gameObject.SetActive(false);
        }
    }

    public List<ShareableObj> PopulateAndReturnShareableList()
    {
        List<ShareableObj> retVal = new List<ShareableObj>();

        ShareableObj[] objs = GetComponentsInChildren<ShareableObj>();

        foreach(ShareableObj o in objs)
        {
            retVal.Add(o);
        }

        foreach(WeaponReferenceBase w in Weapons)
        {
            if(w.holsterWeapon)
            {
                bool wasActive = w.holsterWeapon.activeInHierarchy;
                
                w.holsterWeapon.SetActive(true);
                
                if(w.holsterWeapon.GetComponent<ShareableObj>())
                {
                    ShareableObj o = w.holsterWeapon.GetComponent<ShareableObj>();

                    if (!retVal.Contains(o))
                    {
                        retVal.Add(o);
                    }
                }

                if (!wasActive)
                    w.holsterWeapon.SetActive(false);
            }

            if (w.weaponModel)
            {
                bool wasActive = w.weaponModel.activeInHierarchy;

                w.weaponModel.SetActive(true);

                if (w.weaponModel.GetComponent<ShareableObj>())
                {
                    ShareableObj o = w.weaponModel.GetComponent<ShareableObj>();

                    if (!retVal.Contains(o))
                    {
                        retVal.Add(o);
                    }
                }

                if(!wasActive)
                    w.weaponModel.SetActive(false);
            } 
        }

        foreach(ShareableObj o in additionalShareableObj)
        {
            retVal.Add(o);
        }

        return retVal;
    }
}

