using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TPC;
using UI;
using Weapons.Modifications;

namespace Manager
{
    public class UIManager : MonoBehaviour
    {
        public GameObject menuCanvas;
        public GameObject mainMenu_UI;
        public GameObject gameMenu_UI;
        public GameObject logo;
        public GameObject profileUI;
        public GameObject nameObject;
        public GameObject soloMenu;

        public Text nameText;
        public InputField nameInputField;

        public Transform characterSelectGrid;

        //v2
        public GameObject weaponModUI;
        public WeaponModScene modSceneReferences;

        void Start()
        {
            if(SessionMaster.singleton.debugMode)
            {
                menuCanvas.SetActive(false);               
                return;
            }

            ResourcesManager rm = ResourcesManager.singleton;
            GameObject scbPrefab = Resources.Load("selectCharacterButton") as GameObject;

            for (int i = 0; i < rm.charPrefabs.Count; i++)
            {
                GameObject go = Instantiate(scbPrefab) as GameObject;
                SelectCharacterButton sc = go.GetComponent<SelectCharacterButton>();
                sc.Init(rm.charPrefabs[i].charId);
                go.transform.SetParent(characterSelectGrid);
            }

            //v2
            weaponModUI.SetActive(false);
            modSceneReferences.gameObject.SetActive(false);

            logo.SetActive(false);
            mainMenu_UI.SetActive(false);
            profileUI.SetActive(false);
            soloMenu.SetActive(false);
            nameObject.SetActive(false);
        }

        public void ProfileMenu()
        {
            soloMenu.SetActive(false);
            gameMenu_UI.SetActive(false);
            mainMenu_UI.SetActive(false);
            profileUI.SetActive(true);
            logo.SetActive(false);
            nameObject.SetActive(false);
            weaponModUI.SetActive(false);
        }

        public void FromGameToMenu()
        {
            SessionMaster.singleton.LoadLevel("menu");
        }

        public void FromMenuToSolo()
        {
            soloMenu.SetActive(true);
            gameMenu_UI.SetActive(false);
            mainMenu_UI.SetActive(false);
            profileUI.SetActive(false);
            logo.SetActive(false);
            nameObject.SetActive(false);
            weaponModUI.SetActive(false);
        }

        public void BackToMenu()
        {
            logo.SetActive(true);
            mainMenu_UI.SetActive(true);
            profileUI.SetActive(false);
            nameObject.SetActive(true);
            gameMenu_UI.SetActive(false);
            soloMenu.SetActive(false);
            weaponModUI.SetActive(false);
        }

        public void NewGame()
        {
            PlayerProfile p = SessionMaster.singleton.GetProfile();
            p.currentLevel = "test_scene";
            LoadScene();
        }

        public void LoadScene()
        {
            PlayerProfile p = SessionMaster.singleton.GetProfile();
            SessionMaster.singleton.loadingFromMenu = true;
            SessionMaster.singleton.LoadLevel(p.currentLevel);
        }

        public void OnWriteName()
        {
            SessionMaster.singleton.GetProfile().playerName = nameInputField.text;
            nameText.text = nameInputField.text;
        }

        public void SaveProfileHook()
        {
            SessionMaster.singleton.SaveProfile();
        }

        static public UIManager singleton;

        void Awake()
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
