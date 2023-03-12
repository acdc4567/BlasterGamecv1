using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Weapons.Modifications
{
    public class WeaponModScene : MonoBehaviour
    {
        public Weapon activeWeapon;
        WeaponModelHook modelHook;

        public string rigType = "default";
        public GameObject holder;
        public Transform weaponPlace;
        public Transform cameraPlace;
        Transform currentWeapon;
        [HideInInspector]
        public Transform camTrans;
        Transform prevCameraParent;
        public bool enable;

        public TPC.weaponCategory activeCategory;
        public GameObject categoryHolder;
        public Text categorySelectName;//primary/secondary etc.
        public GameObject subMenusHolder;
        GameObject categoryButtonPrefab;
        WeaponMod_Resources wmR;

        List<WModBase> optics = new List<WModBase>();
        List<WModBase> barrel = new List<WModBase>();
        List<WModBase> rail = new List<WModBase>();
        List<WModBase> magazine = new List<WModBase>();

        public WeaponMod_SubMenu optics_sm;
        public WeaponMod_SubMenu barrel_sm;
        public WeaponMod_SubMenu rail_sm;
        public WeaponMod_SubMenu magazine_sm;

        GameObject currentOptic;
        GameObject currentBarrel;
        GameObject currentRail;
        GameObject currentMagazine;

        public GameObject subMenuButtonPrefab;

        public Text primaryText;
        public Text secondaryText;

        List<WeaponMod_WeaponSelect> weaponSelectButton = new List<WeaponMod_WeaponSelect>();

        CarryingWeapons currentWeapons;

        public void Init(CarryingWeapons wmCon)
        {
            if(wmR == null)
                wmR = WeaponMod_Resources.singleton;

            if (string.IsNullOrEmpty(rigType))
                rigType = "default";

            currentWeapons = wmCon;
            activeCategory = TPC.weaponCategory.primary;
            SetupCategory();
            activeWeapon = wmCon.primary.activeWeapon;
            OpenWeaponModUI(activeWeapon);
            UpdateAllMods(currentWeapons.primary);

            camTrans = Camera.main.transform;
            prevCameraParent = camTrans.parent;
            camTrans.parent = cameraPlace;
            camTrans.localPosition = Vector3.zero;
            camTrans.localRotation = Quaternion.identity;
        }

        public CarryingWeapons CloseWeaponModUI()
        {
            return currentWeapons;
        }

        public void SetupCategory()
        {
            if (categoryButtonPrefab == null)
                categoryButtonPrefab = Resources.Load("categoryButton") as GameObject;

            List<TPC.WeaponContainer> targetList = new List<TPC.WeaponContainer>();
            switch (activeCategory)
            {
                case TPC.weaponCategory.primary:
                    categorySelectName.text = "PRIMARY";
                    targetList = TPC.ResourcesManager.singleton.primary;
                    OpenWeaponModUI(currentWeapons.primary.activeWeapon);
                    UpdateAllMods(currentWeapons.primary);
                    break;
                case TPC.weaponCategory.secondary:
                    categorySelectName.text = "SECONDARY";
                    targetList = TPC.ResourcesManager.singleton.secondary;
                    OpenWeaponModUI(currentWeapons.secondary.activeWeapon);
                    UpdateAllMods(currentWeapons.secondary);
                    break;
            }

            CloseAllButtons();

            for (int i = 0; i < targetList.Count; i++)
            {
                if (i > weaponSelectButton.Count - 1)
                {
                    GameObject go = Instantiate(categoryButtonPrefab) as GameObject;
                    go.transform.SetParent(categoryHolder.transform);
                    WeaponMod_WeaponSelect ws = go.GetComponent<WeaponMod_WeaponSelect>();
                    ws.Init(targetList[i].weaponId);
                    weaponSelectButton.Add(ws);
                }
                else
                {
                    weaponSelectButton[i].Init(targetList[i].weaponId);
                    weaponSelectButton[i].gameObject.SetActive(true);
                }
            }
        }

        void CloseAllButtons()
        {
            foreach (WeaponMod_WeaponSelect w in weaponSelectButton)
            {
                w.gameObject.SetActive(false);

            }
        }

        public void RequestWeapon(string id)
        {
            WeaponInstance instance = TPC.ResourcesManager.singleton.GetWeapon(id, rigType);
            if (instance == null)
            {
                Debug.Log("Weapon with id " + id + " can't be found! Check ids for the weapon and rigtype!");
                return;
            }

            OpenWeaponModUI(instance.instance);
        }

        void OpenWeaponModUI(Weapon weapon)
        {
            activeWeapon = weapon;
            ClearLists();
            PlaceWeapon(activeWeapon.modelPrefab);
            WeaponMod_Hook m = activeWeapon.weaponStats.modificationsHook;

            if (m == null || modelHook == null)
            {
                return;
            }

            if (m.availableMods.Length > 0)
            {
                SetupModUI(m);
            }

            switch (activeCategory)
            {
                case TPC.weaponCategory.primary:
                    primaryText.text = weapon.weaponId.ToUpper();
                    currentWeapons.primary.activeWeapon = weapon;
                    currentWeapons.primary.weaponId = weapon.weaponId;
                    break;
                case TPC.weaponCategory.secondary:
                    secondaryText.text = weapon.weaponId.ToUpper();
                    currentWeapons.secondary.activeWeapon = weapon;
                    currentWeapons.secondary.weaponId = weapon.weaponId;
                    break;
            }          
        }

        void UpdateAllMods(WeaponWithModsContainer c)
        {
            if (!string.IsNullOrEmpty(c.optic))
                SelectMod(c.optic);

            if (!string.IsNullOrEmpty(c.rail))
                SelectMod(c.rail);

            if (!string.IsNullOrEmpty(c.barrel))
                SelectMod(c.barrel);

            if (!string.IsNullOrEmpty(c.magazine))
                SelectMod(c.magazine);
        }

        void SetupModUI(WeaponMod_Hook m)
        {
            if (wmR == null)
                wmR = WeaponMod_Resources.singleton;

            for (int i = 0; i < m.availableMods.Length; i++)
            {
                WModBase w = wmR.GetMod(m.availableMods[i]);

                if (w != null)
                {
                    switch (w.modType)
                    {
                        case WModType.optic:
                            optics.Add(w);
                            break;
                        case WModType.barrel:
                            barrel.Add(w);
                            break;
                        case WModType.rail:
                            rail.Add(w);
                            break;
                        case WModType.siderail:
                            // rail.Add(w);
                            break;
                        case WModType.magazine:
                            magazine.Add(w);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Debug.Log("Weapon mod with id " + m.availableMods[i] + " is missing");
                }
            }

            //Update the buttons
            optics_sm.Init(optics);
            barrel_sm.Init(barrel);
            magazine_sm.Init(magazine);
            rail_sm.Init(rail);
        }

        void ClearLists()
        {
            optics.Clear();
            barrel.Clear();
            rail.Clear();
            magazine.Clear();
            //siderail.Clear();

            optics_sm.Disable();
            rail_sm.Disable();
            magazine_sm.Disable();
            barrel_sm.Disable();
        }

        public void PlaceWeapon(GameObject obj)
        {
            if (currentWeapon != null)
            {
                Destroy(currentWeapon.gameObject);
            }

            GameObject go = Instantiate(obj) as GameObject;
            Transform[] childs = go.GetComponentsInChildren<Transform>();

            foreach (Transform t in childs)
            {
                t.gameObject.layer = LayerMask.NameToLayer("WeaponMods");
            }

            currentWeapon = go.transform;
            currentWeapon.parent = weaponPlace.transform;
            currentWeapon.localRotation = Quaternion.identity;
            currentWeapon.localPosition = Vector3.zero;

            modelHook = currentWeapon.GetComponent<WeaponModelHook>();
        }

        public void SelectMod(string id)
        {
            WeaponMod_instance instance = WeaponMod_Resources.singleton.GetModInstance(id);

            if (instance == null)
                return;

            GameObject go = Instantiate(instance.instance.visPrefab) as GameObject;

            Transform[] childs = go.GetComponentsInChildren<Transform>();

            foreach (Transform t in childs)
            {
                t.gameObject.layer = LayerMask.NameToLayer("WeaponMods");
            }
            
            Transform parent = modelHook.GetMod(instance.instance.modType).place;
            go.transform.parent = parent;
            go.transform.localPosition = instance.instance.localPosition;
            go.transform.localEulerAngles = instance.instance.localEuler;
            go.transform.localScale = instance.instance.localScale;

            switch (activeCategory)
            {
                case TPC.weaponCategory.primary:
                    SaveCurrent(currentWeapons.primary, go, instance.instance.modType, id);
                    break;
                case TPC.weaponCategory.secondary:
                    SaveCurrent(currentWeapons.secondary, go, instance.instance.modType, id);
                    break;
                    /*case TPC.weaponCategory.explosive:
                        break;
                    case TPC.weaponCategory.other:
                        break;
                    default:
                        break;*/
            }

        }

        void SaveCurrent(WeaponWithModsContainer c , GameObject go , WModType modType , string id)
        {
            switch (modType)
            {
                case WModType.optic:
                    if (currentOptic != null)
                        Destroy(currentOptic);
                    currentOptic = go;
                    c.optic = id;
                    break;
                case WModType.barrel:
                    if (currentBarrel != null)
                        Destroy(currentBarrel);
                    currentBarrel = go;
                    c.barrel = id;
                    break;
                case WModType.rail:
                    if (currentRail != null)
                        Destroy(currentRail);
                    currentRail = go;
                    c.rail = id;
                    break;
                case WModType.magazine:
                    if (currentMagazine != null)
                        Destroy(currentMagazine);
                    currentMagazine = go;
                    c.magazine = id;
                    break;
            }
        }

        public void RemoveMod(WModType t)
        {
            switch (activeCategory)
            {
                case TPC.weaponCategory.primary:
                    RemoveModFrom(currentWeapons.primary, t);
                    break;
                case TPC.weaponCategory.secondary:
                    RemoveModFrom(currentWeapons.secondary, t);
                    break;
                /*case TPC.weaponCategory.explosive:
                    break;
                case TPC.weaponCategory.other:
                    break;
                default:
                    break;*/
            }
        }

        void RemoveModFrom(WeaponWithModsContainer c, WModType t)
        {
            switch (t)
            {
                case WModType.optic:
                    if (currentOptic != null)
                        Destroy(currentOptic);
                    c.optic = string.Empty;
                    break;
                case WModType.barrel:
                    if (currentBarrel != null)
                        Destroy(currentBarrel);
                    c.barrel = string.Empty;
                    break;
                case WModType.rail:
                    if (currentRail != null)
                        Destroy(currentRail);
                    c.rail = string.Empty;
                    break;
                case WModType.magazine:
                    if (currentMagazine != null)
                        Destroy(currentMagazine);
                    c.magazine = string.Empty;
                    break;
            }
        }

        public void BackButton()
        {
            TPC.InputHandler.singleton.DisableWeaponMod();
        }

        static public WeaponModScene singleton;
        void Awake()
        {
            singleton = this;
            if (subMenuButtonPrefab == null)
                subMenuButtonPrefab = Resources.Load("modButton") as GameObject;

            DontDestroyOnLoad(this.gameObject);
        }
    }

    [System.Serializable]
    public class CarryingWeapons
    {
        public WeaponWithModsContainer primary;
        public WeaponWithModsContainer secondary;
    }

    [System.Serializable]
    public class WeaponWithModsContainer
    {
        public Weapon activeWeapon;
        public string weaponId;
        public string optic;
        public string barrel;
        public string rail;
        public string magazine;
    }

}
