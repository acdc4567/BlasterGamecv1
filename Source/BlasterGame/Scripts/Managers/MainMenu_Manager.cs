using UnityEngine;
using System.Collections;
using TPC;

namespace Manager
{
    public class MainMenu_Manager : MonoBehaviour
    {
        public Transform characterPlacer;

        [HideInInspector]
        public string targetCharId;
        GameObject charInstance;
        Manager.Menu.MainMenu_Animator mmAnim;
        public AnimatorOverrideController mainMenuAnimator;

        void Start()
        {
            //LoadSettings
            UIManager.singleton.nameText.text = SessionMaster.singleton.GetProfile().playerName;
            targetCharId = SessionMaster.singleton.GetProfile().charId;
            UIManager.singleton.nameInputField.text = SessionMaster.singleton.GetProfile().playerName;
            LoadCharacter();
        }

        public void LoadCharacter()
        {
            if (charInstance != null)
                Destroy(charInstance);

            CharContainer charContainer = ResourcesManager.singleton.GetChar(targetCharId);
            charInstance = Instantiate(charContainer.prefab) as GameObject;
            charInstance.transform.position = characterPlacer.position;
            charInstance.transform.rotation = characterPlacer.rotation;
            charInstance.GetComponent<Animator>().runtimeAnimatorController = mainMenuAnimator;
            SessionMaster.singleton.GetProfile().charId = targetCharId;

            charInstance.AddComponent<BoneHelpers>();
            charInstance.AddComponent<Manager.Menu.MainMenu_Animator>();
            mmAnim = charInstance.GetComponent<Manager.Menu.MainMenu_Animator>();
            mmAnim.Init(charContainer.rig);
        }

        static public MainMenu_Manager singleton;
        void Awake()
        {
            singleton = this;
        }
    }
}
