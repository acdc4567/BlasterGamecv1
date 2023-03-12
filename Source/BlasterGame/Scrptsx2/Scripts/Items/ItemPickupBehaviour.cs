using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ItemPickupBehaviour : MonoBehaviour {

    public ItemsBase itemToPickup;
    WeaponManager wm;
    Text UItext;
    bool initItem;

    WeaponItem wpToPickup;
    AmmoItem amItemToPick;

    void Start()
    {
        UItext = CrosshairManager.GetInstance().pickItemsText;
        wm = GetComponent<WeaponManager>();
        UItext.gameObject.SetActive(false);
    }

	void Update () {

        CheckItemType();
        ActualPickup();
	}

    void CheckItemType()
    {
        if (itemToPickup != null)
        {
            if (!initItem)
            {
                UItext.gameObject.SetActive(true);

                switch (itemToPickup.itemType)
                {
                    case ItemsBase.ItemType.weapon:
                        WeaponItemPickup();
                        break;
                    case ItemsBase.ItemType.ammo:
                        AmmoItemPickup();
                        break;
                    default:
                        break;
                }

                initItem = true;
            }
        }
        else
        {
            if (initItem)
            {
                initItem = false;
                wpToPickup = null;
                amItemToPick = null;
                UItext.gameObject.SetActive(false);
            }
        }
    }

    void ActualPickup()
    {
        if (Input.GetKey(KeyCode.X))
        {
            WeaponActualPickup();
            AmmoItemActualPickup();
        }
    }

    void WeaponItemPickup()
    {
        wpToPickup = itemToPickup.GetComponent<WeaponItem>();

        string targetId = wpToPickup.weaponId;

        if(wm.AvailableWeapons.Count < wm.maxWeapons)
        {
            UItext.text = "Press X to Pick up " + targetId;
        }
        else
        {
            UItext.text = "Press X to Switch " + wm.ReturnCurrentWeapon().weaponID + " with " + targetId;
        }

    }

    void WeaponActualPickup()
    {
        if (wpToPickup != null)
        {
            WeaponReferenceBase targetWeapon = wm.ReturnWeaponWithID(wpToPickup.weaponId);

            if (targetWeapon != null)
            {
                wm.AvailableWeapons.Add(targetWeapon);

                if (targetWeapon.holsterWeapon)
                    targetWeapon.holsterWeapon.SetActive(true);

                if (wm.AvailableWeapons.Count >= wm.maxWeapons + 1)
                {
                    WeaponReferenceBase prevWeapon = wm.AvailableWeapons[wm.weaponIndex];

                    wm.AvailableWeapons.Remove(prevWeapon);

                    if (!wm.states.meleeWeapon)
                        wm.SwitchWeaponWithTargetWeapon(targetWeapon);

                    if (prevWeapon.pickablePrefab != null)
                    {
                        Instantiate(prevWeapon.pickablePrefab,
                            (transform.position + transform.forward * 2) + Vector3.up,
                            Quaternion.identity);

                        if (prevWeapon.holsterWeapon)
                            prevWeapon.holsterWeapon.SetActive(false);
                    }
                }
            }

            Destroy(wpToPickup.gameObject);
            wpToPickup = null;
            itemToPickup = null;
        }
    }

    void AmmoItemPickup()
    {
        amItemToPick = itemToPickup.GetComponent<AmmoItem>();

        WeaponReferenceBase forWp = wm.ReturnWeaponWithID(amItemToPick.weaponId);

        //if the ammo belongs to a weapon we have
        if(wm.AvailableWeapons.Contains(forWp))
        {
            //and that weapon hasn't reached full ammo capabilities
            if(forWp.carryingAmmo < forWp.maxAmmo)
            {
                UItext.text = "Press X to Pick up Ammo for " + amItemToPick.weaponId;
            }
            else
            {
                UItext.text = "Ammo for " + amItemToPick.weaponId + " is full";
            }
            
        }
        else
        {
            UItext.text = "Can't pickup ammo for " + amItemToPick.weaponId;
        }
    }

    void AmmoItemActualPickup()
    {
        if (amItemToPick != null)
        {
            WeaponReferenceBase targetWeapon = wm.ReturnWeaponWithID(amItemToPick.weaponId);

            if (targetWeapon != null)
            {
                if (targetWeapon.carryingAmmo < targetWeapon.maxAmmo)
                {
                    targetWeapon.carryingAmmo += amItemToPick.ammoAmount;

                    if (targetWeapon.carryingAmmo > targetWeapon.maxAmmo)
                    {
                        targetWeapon.carryingAmmo = targetWeapon.maxAmmo;
                    }

                    Destroy(amItemToPick.gameObject);
                    amItemToPick = null;
                    itemToPickup = null;
                }
            }  
        }
    }
}
