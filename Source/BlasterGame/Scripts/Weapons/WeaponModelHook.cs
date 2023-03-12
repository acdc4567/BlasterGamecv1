using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TPC.ObjectPool;
using TPC;
using Manager;
using Weapons.Modifications;

namespace Weapons
{
    public class WeaponModelHook : MonoBehaviour
    {
        Animator anim;
        [HideInInspector]
        public ParticleSystem[] otherParticles;

        public GameObject magazineOnWeapon;
        [Header("The magazine object with physics")]
        public string magazinePrefab;
        public string weaponId;
        [Header("The magazine object on the character's hand")]
        public string magazineItemId;
        GameObject magazineItem;

        //V2
        WeaponModManager modManager;
        public List<string> activeMods = new List<string>();
        public WM_Place[] mod_placement;
        [HideInInspector]
        public RuntimeWeapon runtimeWeapon;

        public WM_Place GetMod(WModType type)
        {
            WM_Place r = null;

            for (int i = 0; i < mod_placement.Length; i++)
            {
                if (mod_placement[i].type == type)
                {
                    r = mod_placement[i];
                    break;
                }
            }

            return r;
        }

        public BoneHelpers boneHelper;
        string modelRig;

        bool createAudio = false;
        AudioSource audioSource;

        public StringHolder stringHolder;

        public void Init(BoneHelpers bh, string targetRig, RuntimeWeapon rw)
        {
            modelRig = targetRig;
            //v2
            runtimeWeapon = rw;
            anim = GetComponentInChildren<Animator>();
            otherParticles = GetComponentsInChildren<ParticleSystem>();
            boneHelper = bh;
            //v2
            modManager = GetComponent<WeaponModManager>();

            CreateMagazineInstance();

            if(createAudio)
            {
                gameObject.AddComponent<AudioSource>();
                audioSource = GetComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            //v2
            InitMods();
        }

        //v2
        public void InitMods()
        {
            modManager.ClearAllContainers();

            for (int i = 0; i < activeMods.Count; i++)
            {
                modManager.AddMod(activeMods[i]);
            }
        }

        public void ThrowMagazine()
        {
            GameObject go = ObjectPool.singleton.RequestObject(magazinePrefab);
            if (go == null)
                return;

            go.transform.position = magazineOnWeapon.transform.position;

            Rigidbody rb = go.GetComponent<Rigidbody>();

            if (rb == null)
                return;

            rb.velocity = Vector3.zero;
            Vector3 direction = transform.root.forward;
            rb.AddRelativeTorque((-Vector3.forward * 5) + (Vector3.right * TPC.Statics.RandomFloat(-2,2)));
            rb.AddForce(direction * 3, ForceMode.Impulse);
        }

        public void Fire()
        {
            if(anim != null)
            {
                if(anim.isInitialized)
                {
                    anim.SetBool(TPC.Statics.shoot, true);
                }
            }

            foreach (ParticleSystem p in otherParticles)
            {
                p.Emit(1);
            }

            if(createAudio)
            {
                AudioFX fx = AudioManager.singleton.GetAudio(stringHolder.gunshot);
                audioSource.clip = fx.audioClip;
                audioSource.Play();
            }
        }

        void CreateMagazineInstance()
        {
            if (string.IsNullOrEmpty(magazineItemId))
                return;

            ItemInstance ic = ResourcesManager.singleton.GetItem(magazineItemId, modelRig);
            if (ic == null)
                return;

            magazineItem = Instantiate(ic.instance.modelPrefab) as GameObject;
            Transform b = boneHelper.ReturnHelper(ic.instance.bone).helper;
            magazineItem.transform.parent = b;
            magazineItem.transform.localPosition = ic.instance.localPosition;
            magazineItem.transform.localEulerAngles = ic.instance.localEuler;
            magazineItem.transform.localScale = ic.instance.localScale;
            magazineItem.SetActive(false);
        }

        public void OpenMagazineOnHand()
        {
            if (magazineItem)
                magazineItem.SetActive(true);
        }

        public void CloseMagazineOnHand()
        {
            if(magazineItem)
                magazineItem.SetActive(false);
        }
    }

    [System.Serializable]
    public class StringHolder
    {
        public string reload;
        public string gunshot;
        public string handle;
    }
}
