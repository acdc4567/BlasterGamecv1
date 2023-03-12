using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [Header("The ui elements for loading")]
    public bool loading;
    public Text loadText;
    public GameObject loadUI;
    
    [Header("The script that controls our select models script")]
    public SelectModel_Menu select_m_Menu;

    [HideInInspector]
    public string targetModelId;
    
    [Header("The model for the main menu")]
    public StateManager selectScreenModel;

    float loadT;
    int index;
    string dot = ".";

    [HideInInspector]
    public bool enableMenu = true;
    [HideInInspector]
    public bool inGameMenu;

    [Header("The main menu")]
    public GameObject mainMenuGrid;

    [Header("Add all cases for buttons here and their status")]
    public List<MenuButton> mButtons = new List<MenuButton>();

    public GameObject backButton;
    GameObject curActiveMenu;

    [Header("Freeze game time when on menu")]
    public bool freezeOnMenu;

    void Start()
    {
        //Only have 1 object with a state manager in the main menu
        selectScreenModel = FindObjectOfType<StateManager>();

        curActiveMenu = mainMenuGrid;
        mmStatus = MMStatus.main;

        OpenButtons();
    }

    public void OpenButtons()
    {
        CloseAllButtons();

        foreach (MenuButton b in mButtons)
        {
            if (b.enabled)
            {
                if (b.forMenu == mmStatus || b.forMenu == MMStatus.all)
                {
                    b.go.SetActive(true);
                    b.go.transform.SetSiblingIndex(b.preferedPosition);
                }
            }
        }
    }

    void Update()
    {
        if (loading)
        {
            enableMenu = false;

            loadUI.SetActive(true);

            loadT += Time.deltaTime;

            if (loadT > 0.5f)
            {
                loadT = 0;

                if (index < 3)
                {
                    loadText.text += dot;
                    index++;
                }
                else
                {
                    loadText.text = "Loading";
                    index = 0;
                }
            }
        }
        else
        {
            enableMenu = true;
            loadUI.SetActive(false);
        }

        switch(mmStatus)
        {
            case MMStatus.main:
                curActiveMenu.SetActive(enableMenu);   
                break;
            case MMStatus.inGame:
                curActiveMenu.SetActive( (!loading)? inGameMenu : false); 
                break;
        }
         
        if(inGameMenu && freezeOnMenu)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    void CloseAllButtons()
    {
        foreach(MenuButton b in mButtons)
        {
            b.go.SetActive(false);
        }
        
        backButton.SetActive(false);
    }

    public MMStatus mmStatus;
    public enum MMStatus
    {
        main,
        inGame,
        all
    }

    #region Button functions

    public void StartNewGame()
    {
        ChangeCurrentActiveMenu(select_m_Menu.gameObject);
        select_m_Menu.Init();
        backButton.SetActive(true);
    }

    public void SwitchModel(string id)
    {
        ResourceManager.GetInstance().SwitchCharacterModelWithId(selectScreenModel, id);
    }

    public void LoadLevel()
    {
        backButton.SetActive(false);
        ChangeCurrentActiveMenu(mainMenuGrid);
        mmStatus = MMStatus.inGame;

        loading = true;
        StartCoroutine(LoadALevel(1));
    }
 
    public void OpenMainMenu()
    {
        ChangeCurrentActiveMenu(mainMenuGrid);    
        backButton.SetActive(false);
    }

    public void LoadMainMenu()
    {
        enableMenu = false;
        inGameMenu = false;
        ChangeCurrentActiveMenu(mainMenuGrid);
        mmStatus = MMStatus.main;

        loading = true;
        StartCoroutine(LoadALevel(0));
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadALevel(int targetLevel)
    {
        yield return SceneManager.LoadSceneAsync(targetLevel);
        loading = false;
    }

    void ChangeCurrentActiveMenu(GameObject targetMenu)
    {
        curActiveMenu.SetActive(false);
        curActiveMenu = targetMenu;
        curActiveMenu.SetActive(true);
    }

    #endregion

    public static MainMenu instance;
    public static MainMenu GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

}

[System.Serializable]
public class MenuButton
{
    public int preferedPosition;
    public GameObject go;
    public bool enabled = true;
    public MainMenu.MMStatus forMenu = MainMenu.MMStatus.main;
}