using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TPC;

namespace Manager
{
    public class SessionMaster : MonoBehaviour
    {
        PlayerProfile activeProfile;   
        string targetScene;
        public bool debugMode;
        public bool useDebugValues;
        public DebugValues debugValues;
        public bool prewarm;

        [HideInInspector]
        public bool isMultiplayer;

        void Start()
        {
            if(debugMode)
            {
                Debug.Log("Debug mode is on!");
                return;
            }

            List<PlayerProfile> availableProfiles = Serializer.singleton.GetProfiles();

            if(availableProfiles.Count == 0)
            {
                activeProfile = new PlayerProfile();
                activeProfile.playerName = System.Environment.UserName;
                activeProfile.charId = "default";
                activeProfile.progression = 0;
            }
            else
            {
                activeProfile = availableProfiles[0];
            }

            if (useDebugValues)
            {
                if (!string.IsNullOrEmpty(debugValues.mainWeapon))
                    activeProfile.mainWeapon = debugValues.mainWeapon;

                if (!string.IsNullOrEmpty(debugValues.secWeapon))
                    activeProfile.secWeapon = debugValues.secWeapon;

                if (debugValues.progression != -1)
                    activeProfile.progression = debugValues.progression;
            }

            StartCoroutine("StartGame");           
        }

        public PlayerProfile GetProfile()
        {
            return activeProfile;
        }
     
        public void SaveProfile()
        {
            Serializer.singleton.SaveProfile(activeProfile);
        }

        IEnumerator StartGame()
        {
            yield return LoadDependencies();
            if (prewarm)
                yield return new WaitForSeconds(0.4f);
            yield return LoadMenu();
        }

        IEnumerator LoadDependencies()
        {
            yield return SceneManager.LoadSceneAsync("dependencies", LoadSceneMode.Single);
        }

        IEnumerator LoadMenu()
        {
            yield return SceneManager.LoadSceneAsync("menu", LoadSceneMode.Single);
            Time.timeScale = 1;
            UIManager.singleton.BackToMenu();
            UIManager.singleton.menuCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void LoadLevel(string lvl)
        {
            if(useDebugValues)
            {
                if (!string.IsNullOrEmpty(debugValues.debugScene))
                    lvl = debugValues.debugScene;
            }

            if (string.IsNullOrEmpty(lvl))
                targetScene = "test_scene";
            else
                targetScene = lvl;

            if(targetScene == "menu")
            {
                StartCoroutine("LoadMenu");
                return;
            }

            StartCoroutine("LoadTargetLevel");
        }

        IEnumerator LoadLevelDependencies()
        {
            yield return SceneManager.LoadSceneAsync("level_dependencies", LoadSceneMode.Single);
        }

        [HideInInspector]
        public bool loadingFromMenu;

        IEnumerator LoadTargetLevel()
        {
            ResourcesManager.singleton.ResetResources();
            yield return LoadLevelDependencies();
            yield return LoadLoadingScene();
            yield return new WaitForSeconds(2);//Artificial loading, you don't need this for the final build
            yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            UIManager.singleton.menuCanvas.SetActive(false);
            SceneManager.UnloadSceneAsync("loading");

            if(loadingFromMenu)
            {
                LevelObjectives.singleton.takePositionFromProfile = true;
            }
            else
            {
                GetProfile().progression = 0;
            }

            GetProfile().currentLevel = targetScene;
            loadingFromMenu = false;
        }

        IEnumerator LoadLoadingScene()
        {
            yield return SceneManager.LoadSceneAsync("loading", LoadSceneMode.Additive);
        }

        static public SessionMaster singleton;

        void Awake()
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [System.Serializable]
    public class PlayerProfile
    {
        public string playerName;
        public string charId = "def";
        public string mainWeapon = "m4";
        public string secWeapon = "b8";
        public List<string> mainWeaponMods = new List<string>();
        public List<string> secWeaponMods = new List<string>();
        public string currentLevel = "";
        public int progression = 0;
        public float px;
        public float py;
        public float pz;
        public float prx;
        public float pry;
        public float prz;
    }

    [System.Serializable]
    public class DebugValues
    {
        public string mainWeapon;
        public string secWeapon;
        public string debugScene;
        [Header("-1 to disable")]
        public int progression = -1;
    }
}
