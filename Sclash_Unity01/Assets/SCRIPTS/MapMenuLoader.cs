﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This script manages loading and reading of the elements, settings and saves of the stages menu
// OPTIMIZED ?
public class MapMenuLoader : MonoBehaviour
{
    #region VARIABLES
    [Header("MANAGERS")]
    [SerializeField] MapLoader mapLoader = null;



    #region DATA
    [Header("DATA")]
    [Tooltip("The reference to the scriptable object data containing all the maps")]
    [SerializeField] MapsDataBase mapsDatabase01 = null;
    [SerializeField] MenuParameters parametersData = null;
    #endregion


    #region STAGE MODE
    [Header("STAGE MODE")]
    [SerializeField] List<string> stagesModes = new List<string>() { "all", "day", "night", "custom" };
    [SerializeField] int currentStageMode = 0;
    [SerializeField] List<GameObject> stageModeDisplayObjects = null;
    #endregion


    #region INPUT
    [Header("INPUT")]
    [SerializeField] string stageModeSwitchAxis = "MenuTriggers";
    [SerializeField] float stageModeSwitchAxisDeadzone = 0.3f;
    bool canInputModeChange = true;
    bool hasFinishedChangingMode = true;
    #endregion



    #region STAGES LIST
    [Header("STAGE LIST")]
    [Tooltip("The reference to the game object in which all the menu elements of the maps to choose it will be instantiated")]
    [SerializeField] Transform stagesListParent = null;
    [SerializeField]
    GameObject
        stageButtonObject = null;
    [HideInInspector] public List<int> currentlyDisplayedStagesList = new List<int>();
    #endregion


    [SerializeField] MenuBrowser2D stagesListBrowser2D = null;
    [SerializeField] int numberOfElementsPerLine = 4;



    #region STAGE PARAMETERS
    [SerializeField] GameObject dayNightCycleCheck = null;
    [SerializeField] GameObject randomStageAfterGameCheck = null;
    [SerializeField] GameObject customListForRandomStageAfterGameCheck = null;
    [SerializeField] GameObject keepLastLoadedStageCheck = null;
    [SerializeField] GameObject customListForStartRandomStageCheck = null;
    #endregion



    [Header("AUDIO")]
    [SerializeField] PlayRandomSoundInList clickSoundSource = null;
    #endregion















    PlayerControls controls;

    #region FUNCTIONS
    #region BASE FUNCTIONS
    // Start is called before the first frame update
    void Start()
    {
        controls = GameManager.Instance.Controls;

        ChangeStageMode(0);
    }


    private void OnEnable()
    {
        LoadParameters();
        SetUpMenu();
    }


    void Update()
    {
        if (enabled)
            ManageStageModeChange();
    }
    # endregion








    #region STAGE MODE
    void ManageStageModeChange()
    {
        if (canInputModeChange && hasFinishedChangingMode && Mathf.Abs(controls.Menu.Menutriggers.ReadValue<float>()) > stageModeSwitchAxisDeadzone)
        {
            canInputModeChange = false;


            if (controls.Menu.Menutriggers.ReadValue<float>() < -stageModeSwitchAxisDeadzone)
                ChangeStageMode(-1);
            else if (controls.Menu.Menutriggers.ReadValue<float>() > -stageModeSwitchAxisDeadzone)
                ChangeStageMode(1);
        }
        else if (Mathf.Abs(controls.Menu.Menutriggers.ReadValue<float>()) < stageModeSwitchAxisDeadzone)
            canInputModeChange = true;
    }


    public void ChangeStageMode(int indexIncrementation)
    {
        if (hasFinishedChangingMode)
        {
            hasFinishedChangingMode = false;


            // AUDIO
            if (clickSoundSource != null)
                clickSoundSource.Play();


            currentStageMode += indexIncrementation;


            if (currentStageMode < 0)
                currentStageMode = stagesModes.Count - 1;
            else if (currentStageMode > stagesModes.Count - 1)
                currentStageMode = 0;


            UpdateDisplayedMapListTypeIndication();
            StartCoroutine(LoadStagesList());
        }
    }


    void UpdateDisplayedMapListTypeIndication()
    {
        for (int i = 0; i < stagesModes.Count; i++)
            if (i == currentStageMode)
                stageModeDisplayObjects[i].SetActive(true);
            else
                stageModeDisplayObjects[i].SetActive(false);
    }
    #endregion








    #region STAGES LIST
    void ClearStagesList()
    {
        stageButtonObject.SetActive(false);
        currentlyDisplayedStagesList.Clear();
        stagesListBrowser2D.elements2D = new MenuBrowser2D.ElementsLine[0];


        for (int i = 0; i < stagesListParent.childCount; i++)
            if (stagesListParent.GetChild(i).gameObject.activeInHierarchy)
                Destroy(stagesListParent.GetChild(i).gameObject);
    }


    IEnumerator LoadStagesList()
    {
        // CLEAR LIST
        StopCoroutine(LoadStagesList());
        ClearStagesList();


        // BROWSING
        int numberOfElementsOnThisLine = 0;
        int numberOfLinesForBrowsing = 0;


        for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
            if ((mapsDatabase01.stagesLists[i].type.ToString() == stagesModes[currentStageMode]) || stagesModes[currentStageMode] == "all" || (mapsDatabase01.stagesLists[i].inCustomList && (stagesModes[currentStageMode] == stagesModes[3])))
            {
                numberOfElementsOnThisLine++;


                if (numberOfElementsOnThisLine == numberOfElementsPerLine)
                {
                    numberOfLinesForBrowsing++;
                    stagesListBrowser2D.elements2D = new MenuBrowser2D.ElementsLine[numberOfLinesForBrowsing];

                    for (int y = 0; y < stagesListBrowser2D.elements2D.Length - 1; y++)
                    {
                        stagesListBrowser2D.elements2D[y] = new MenuBrowser2D.ElementsLine();
                        stagesListBrowser2D.elements2D[y].line = new GameObject[numberOfElementsPerLine];
                    }
                    stagesListBrowser2D.elements2D[numberOfLinesForBrowsing - 1] = new MenuBrowser2D.ElementsLine();
                    stagesListBrowser2D.elements2D[numberOfLinesForBrowsing - 1].line = new GameObject[numberOfElementsOnThisLine];

                    numberOfElementsOnThisLine = 0;
                }
            }


        if (numberOfElementsOnThisLine > 0)
        {
            numberOfLinesForBrowsing++;
            stagesListBrowser2D.elements2D = new MenuBrowser2D.ElementsLine[numberOfLinesForBrowsing];
            for (int y = 0; y < stagesListBrowser2D.elements2D.Length - 1; y++)
            {
                stagesListBrowser2D.elements2D[y] = new MenuBrowser2D.ElementsLine();
                stagesListBrowser2D.elements2D[y].line = new GameObject[numberOfElementsPerLine];
            }
            stagesListBrowser2D.elements2D[numberOfLinesForBrowsing - 1] = new MenuBrowser2D.ElementsLine();
            stagesListBrowser2D.elements2D[numberOfLinesForBrowsing - 1].line = new GameObject[numberOfElementsOnThisLine];
            numberOfElementsOnThisLine = 0;
        }


        numberOfElementsOnThisLine = 0;
        numberOfLinesForBrowsing = 0;


        // FILL LIST
        for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
            if ((mapsDatabase01.stagesLists[i].type.ToString() == stagesModes[currentStageMode]) || stagesModes[currentStageMode] == "all" || (mapsDatabase01.stagesLists[i].inCustomList && (stagesModes[currentStageMode] == stagesModes[3])))
            {
                GameObject newMapMenuObject = null;
                MapMenuObject newMapMenuObjectScript = null;


                newMapMenuObject = Instantiate(stageButtonObject, stagesListParent);
                newMapMenuObject.SetActive(true);
                newMapMenuObjectScript = newMapMenuObject.GetComponent<MapMenuObject>();


                newMapMenuObjectScript.mapImage.sprite = mapsDatabase01.stagesLists[i].mapImage;
                newMapMenuObjectScript.mapText.text = mapsDatabase01.stagesLists[i].stageName;
                newMapMenuObjectScript.stageIndex = i;
                newMapMenuObjectScript.UpdateCustomListCheckBox();
                currentlyDisplayedStagesList.Add(i);

                // BROWSING
                stagesListBrowser2D.elements2D[numberOfLinesForBrowsing].line[numberOfElementsOnThisLine] = newMapMenuObjectScript.mapButtonObject;
                numberOfElementsOnThisLine++;
                if (numberOfElementsOnThisLine == numberOfElementsPerLine)
                {
                    numberOfElementsOnThisLine = 0;
                    numberOfLinesForBrowsing++;
                }


                yield return new WaitForSeconds(0.05f);
            }


        hasFinishedChangingMode = true;
        stageButtonObject.SetActive(false);
        stagesListBrowser2D.Select(true);
    }
    #endregion







    # region SAVE / LOAD PARAMETERS
    // Sets up the menu from scriptable object
    public void SetUpMenu()
    {
        dayNightCycleCheck.SetActive(parametersData.dayNightCycle);
        randomStageAfterGameCheck.SetActive(parametersData.randomStage);
        customListForRandomStageAfterGameCheck.SetActive(parametersData.useCustomListForRandom);
        keepLastLoadedStageCheck.SetActive(parametersData.keepLastLoadedStage);
        customListForStartRandomStageCheck.SetActive(parametersData.useCustomListForRandomStartStage);
    }

    public void LoadDemoParameters()
    {
        JsonSave save = SaveGameManager.GetCurrentSave();

        parametersData.dayNightCycle = false;
        parametersData.randomStage = false;
        parametersData.useCustomListForRandom = false;
        parametersData.keepLastLoadedStage = true;
        parametersData.useCustomListForRandomStartStage = false;
        parametersData.lastLoadedStageIndex = save.lastLoadedStageIndex;


        // Favourite stages list
        parametersData.customList = save.customList;

        if (mapsDatabase01 == null)
        {
            Debug.LogWarning("Map database is null");
            return;
        }

        if (parametersData.customList.Count < mapsDatabase01.stagesLists.Count)
        {
            parametersData.customList.Clear();


            for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
                parametersData.customList.Add(mapsDatabase01.stagesLists[i].inCustomList);
        }



        for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
        {
            Map newMap = mapLoader.mapsData.stagesLists[i];


            if (parametersData.customList[i])
                newMap.inCustomList = true;
            else
                newMap.inCustomList = false;


            mapLoader.mapsData.stagesLists[i] = newMap;
        }
    }

    // Load menu parameters save in the scriptable object
    public void LoadParameters()
    {
        // Get save file
        JsonSave save = SaveGameManager.GetCurrentSave();

        // Sets data in scriptable object from the save file
        // Settings
        parametersData.dayNightCycle = save.dayNightCycle;
        parametersData.randomStage = save.randomStage;
        parametersData.useCustomListForRandom = save.useCustomListForRandom;
        parametersData.keepLastLoadedStage = save.keepLastLoadedStage;
        parametersData.useCustomListForRandomStartStage = save.useCustomListForRandomStartStage;
        parametersData.lastLoadedStageIndex = save.lastLoadedStageIndex;


        // Favourite stages list
        parametersData.customList = save.customList;



        // DOES STUFF WITH THE FAVOURITES LIST
        // Checks if map database is referenced
        if (mapsDatabase01 == null)
        {
            Debug.LogWarning("Map database is null");
            return;
        }

        // Favourites list is a list of bool that is supposed to be the same length as the default stages list
        if (parametersData.customList.Count < mapsDatabase01.stagesLists.Count)
        {
            parametersData.customList.Clear();


            for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
                parametersData.customList.Add(mapsDatabase01.stagesLists[i].inCustomList);
        }


        // Sets the favourite attribute in each stage of the built in list depending on the favourite list in the save
        for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
        {
            Map newMap = mapLoader.mapsData.stagesLists[i];


            if (parametersData.customList[i])
                newMap.inCustomList = true;
            else
                newMap.inCustomList = false;


            mapLoader.mapsData.stagesLists[i] = newMap;
        }
    }


    // Saves the map menu parameters in the scriptable object
    void SaveInScriptableObject()
    {
        // Checks the actual UI objects and sets the scriptable object parameters after its
        parametersData.dayNightCycle = dayNightCycleCheck.activeInHierarchy;
        parametersData.randomStage = randomStageAfterGameCheck.activeInHierarchy;
        parametersData.useCustomListForRandom = customListForRandomStageAfterGameCheck.activeInHierarchy;
        parametersData.keepLastLoadedStage = keepLastLoadedStageCheck.activeInHierarchy;
        parametersData.useCustomListForRandomStartStage = customListForStartRandomStageCheck.activeInHierarchy;


        // Favourite stages list
        // Favourites list is a list of bool that is supposed to be the same length as the default stages list
        if (parametersData.customList.Count < mapsDatabase01.stagesLists.Count)
        {
            parametersData.customList.Clear();


            for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
                parametersData.customList.Add(mapsDatabase01.stagesLists[i].inCustomList);
        }

        for (int i = 0; i < mapsDatabase01.stagesLists.Count; i++)
            if (mapsDatabase01.stagesLists[i].inCustomList)
                parametersData.customList[i] = true;
            else
                parametersData.customList[i] = false;
    }

    // Save map menu settings forever from scriptable object data
    public void SaveParameters()
    {
        SaveInScriptableObject();


        JsonSave save = SaveGameManager.GetCurrentSave();


        save.customList = parametersData.customList;
        save.dayNightCycle = parametersData.dayNightCycle;
        save.randomStage = parametersData.randomStage;
        save.useCustomListForRandom = parametersData.useCustomListForRandom;
        save.keepLastLoadedStage = parametersData.keepLastLoadedStage;
        save.useCustomListForRandomStartStage = parametersData.useCustomListForRandomStartStage;
        save.lastLoadedStageIndex = parametersData.lastLoadedStageIndex;


        SaveGameManager.Save();
    }
    #endregion
    #endregion
}
