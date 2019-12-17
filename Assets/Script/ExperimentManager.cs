﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using TMPro;
using UnityEngine.UI;
using System.IO;

public enum GameState
{
    Prepare,
    ShowPattern, // 10s
    SelectCards,
    Result
}

public enum Layout
{
    Flat,
    LimitedFlat,
    FullCircle,
    LimitedFullCircle,
    NULL
}

public class ExperimentManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject CardPrefab;
    public Sprite ShapePrefab1;
    public Text DashBoardText;
    public Text SeenCardText;
    public Text SelectCardText;
    public Text MemoryTypeText;
    public Text TimerText;
    public TextMeshProUGUI LeftControllerText;
    public TextMeshProUGUI RightControllerText;
    public Transform ScreenDashBoard;
    public Transform FilterCube;
    public Transform EdgeIndicator;
    [Header("Task File")]
    public TextAsset Patterns3;
    public TextAsset Patterns5;

    [Header("Predefined Variables")]
    public float hDelta;
    public float vDelta;
    public float cardSize;
    public float memoryTime;
    public int numberOfRows;
    public int numberOfColumns;

    [Header("Variables")]
    public Layout layout;
    public int difficultyLevel;

    /// <summary>
    /// local variables
    /// </summary>

    // do not change
    private List<Text> Instructions;
    [HideInInspector]
    public int mainHand = 1; // 0: left; 1: right
    private float adjustedHeight = 1;
    private Transform mainController;
    private VRTK_InteractUse mainHandIU;
    private VRTK_ControllerEvents mainHandCE;
    private int maxTrialNo = 72;
    // string variables
    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

    // incremental with process
    private GameState gameState;
    private int trialNo = 1;

    // refresh every trail
    private List<GameObject> cards;
    private List<GameObject> selectedCards;
    private bool correctTrial = true; // true if user answer all correct cards
    private float LocalMemoryTime;
    private List<string> LvL3TaskList;
    private List<string> LvL5TaskList;

    // refresh in one process
    private bool showingPattern = false; // show pattern stage
    private bool startCount = false; // show pattern stage
    private bool allSeen = false;
    private float scanTime = 0;
    private bool allSelected = false;
    private float selectTime = 0;
    private int accurateNumber = 0;

    // check on update for interaction
    private bool localTouchpadPressed = false;
    private bool localMenuPressed = false;
    private bool instruction = true;

    // log use
    private string trialID;
    private int experimentSequence;
    private float currentAllSeenTime;
    private float currentAllSelectTime;
    StreamWriter writer;
    StreamWriter writerHead;
    StreamWriter writerAnswer;

    // Start is called before the first frame update
    void Start()
    {
        // initialise variables
        cards = new List<GameObject>();
        selectedCards = new List<GameObject>();

        LvL3TaskList = new List<string>();
        LvL5TaskList = new List<string>();

        Instructions = new List<Text>
        {
            SeenCardText,
            SelectCardText,
            TimerText
        };

        ReadPatternsFromInput();

        // setup main hand
        if (StartSceneScript.controllerHand == 0 || StartSceneScript.controllerHand == 1)
            mainHand = StartSceneScript.controllerHand;
        else
            mainHand = 1;

        // setup adjusted height
        adjustedHeight = StartSceneScript.adjustedHeight;

        // setup trail Number
        if (StartSceneScript.PublicTrialNumber != 0)
            trialNo = StartSceneScript.PublicTrialNumber;

        // setup writer stream
        string writerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_RawData.csv";
        writer = new StreamWriter(writerFilePath, true);

        string writerHeadFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_HeadAndHand.csv";
        writerHead = new StreamWriter(writerHeadFilePath, true);

        string writerAnswerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_Answers.csv";
        writer = new StreamWriter(writerAnswerFilePath, true);

        LocalMemoryTime = memoryTime;
        // setup experiment
        PrepareExperiment();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainController == null) {
            if (mainHand == 0) {
                if (GameObject.Find("Controller (left)") != null)
                    mainController = GameObject.Find("Controller (left)").transform;
            }
            else {
                if (GameObject.Find("Controller (right)") != null)
                    mainController = GameObject.Find("Controller (right)").transform;
            }
        }

        // change layout shortcut
        if (Input.GetKeyDown("c"))
            Changelayout();

        if (gameState == GameState.SelectCards) {
            GameInteraction();
            PrintTextToScreen(DashBoardText, "Total number of white cards: <color=green>" + difficultyLevel + "</color>\nYou have selected: <color=green>" + selectedCards.Count + "</color>\n\nPlease press <color=green>Finish</color> button when you finish.");
        }

        //DetectJoystick();

        if (gameState == GameState.ShowPattern)
            TimerAndCheckScan();

        CheckStateChange();

        WritingToLog();
    }

    // write to log file
    private void WritingToLog() {
        Transform mainLogController = null;

        if (mainHand == 0)
        {
            if (GameObject.Find("Controller (left)") != null)
                mainLogController = GameObject.Find("Controller (left)").transform;
        }
        else
        {
            if (GameObject.Find("Controller (right)") != null)
                mainLogController = GameObject.Find("Controller (right)").transform;
        }

        if (writer != null && Camera.main != null && mainLogController != null)
        {
            SteamVR_TrackedController mainControllerScript = mainLogController.GetComponent<SteamVR_TrackedController>();

            writer.WriteLine(GetFixedTime() + "," + StartSceneScript.adjustedHeight + "," + GetTrialNumber() + "," + GetTrialID() + "," + StartSceneScript.ParticipantID + "," + StartSceneScript.ExperimentSequence + "," +
                GetLayout() + "," + GetDifficulty() + "," + GetGameState() + "," + VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(mainLogController.position) + "," + VectorToString(mainLogController.eulerAngles) + "," + mainControllerScript.menuPressed + "," + mainControllerScript.triggerPressed);
            writer.Flush();
        }

        if (writerHead != null && Camera.main != null)
        {
            writerHead.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + StartSceneScript.ParticipantID + "," + StartSceneScript.ExperimentSequence + "," +
                GetLayout() + "," + GetDifficulty() + "," + GetGameState() + "," + VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(mainLogController.position) + "," + VectorToString(mainLogController.eulerAngles));
            writerHead.Flush();
        }
    }

    // check button pressed for state change
    private void CheckStateChange() {

        // assign left or right controllers controller events
        if (mainHandCE == null)
        {
            if (mainHand == 0 && GameObject.Find("LeftControllerAlias") != null)
                mainHandCE = GameObject.Find("LeftControllerAlias").GetComponent<VRTK_ControllerEvents>();
            else if (mainHand == 1 && GameObject.Find("RightControllerAlias") != null)
                mainHandCE = GameObject.Find("RightControllerAlias").GetComponent<VRTK_ControllerEvents>();
        }
        else {
            if (mainHandCE.touchpadPressed)
            {
                localTouchpadPressed = true;
            }
            else {
                if (localTouchpadPressed) {
                    localTouchpadPressed = false;
                    switch (gameState)
                    {
                        case GameState.Prepare:
                            ShowPattern();
                            break;
                        case GameState.ShowPattern:   
                            break;
                        case GameState.SelectCards:
                            LeftControllerText.text = "Ready";
                            RightControllerText.text = "Ready";
                            FinishAnswering();
                            PrintTextToScreen(DashBoardText, "Great!\nPlease press <color=green>Ready</color> button to set up a new game.");
                            //if (correctTrial)
                            //    PrintTextToScreen(DashBoardText, "<color=green>Correct!</color>\nPlease press <color=green>Ready</color> button to set up a new game.");
                            //else
                            //    PrintTextToScreen(DashBoardText, "<color=red>Wrong!</color>\nPlease press <color=green>Ready</color> button to set up a new game.");
                            break;
                        case GameState.Result:
                            LeftControllerText.text = "Start";
                            RightControllerText.text = "Start";
                            PrepareExperiment();
                            break;
                        default:
                            break;
                    }
                }
            }

            // toggle instructions
            if (mainHandCE.buttonTwoPressed)
            {
                localMenuPressed = true;
            }
            else {
                if (localMenuPressed) {
                    if (instruction)
                    {
                        instruction = false;
                        foreach (Text text in Instructions)
                        {
                            text.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        instruction = true;
                        foreach (Text text in Instructions)
                        {
                            text.gameObject.SetActive(true);
                        }
                    }
                }
                localMenuPressed = false;
            }
        }
    }

    // Prepare stage (after clicking ready button)
    private void PrepareExperiment() {
        gameState = GameState.Prepare;
        LocalMemoryTime = memoryTime;

        if (correctTrial) {
            if (GetTrialID() == "Training")
            {
                PrintTextToScreen(DashBoardText, "This is a training session, you need to remember the position for 3 white cards.\n\nPlease press " +
                    "<color=green>Start</color> button to start the memory game.");
            }
            else {
                PrintTextToScreen(DashBoardText, "Please get ready, your performance will be recorded, you need to remember the position for " + difficultyLevel + 
                    " white cards.\n\nPlease press <color=green>Start</color> button to start the memory game.");
            }
        } 
        else {
            HidePattern(true);
            if (!allSeen)
            {
                PrintTextToScreen(DashBoardText, "You haven't seen all the cards.\n\nPlease press <color=green>Start</color> button to restart a new memory game.");
            }
            else if (!allSelected)
            {
                PrintTextToScreen(DashBoardText, "You haven't selected all the cards.\n\nPlease press <color=green>Start</color> button to start the memory game.");
            }
            else if (!allSelected && !allSeen) {
                PrintTextToScreen(DashBoardText, "You haven't seen and selected all the cards.\n\nYou haven't seen all the cards.\n\n" +
                    "Please press <color=green>Start</color> button to start the memory game.");
            }
        }  

        correctTrial = true;

        // refresh dashboard
        PrintTextToScreen(SeenCardText, "");
        PrintTextToScreen(SelectCardText, "");
        PrintTextToScreen(TimerText, "");
        PrintTextToScreen(MemoryTypeText, "");
 
        // change trial conditions based on trial number
        // layout
        
        layout = (GetCurrentCardsLayout() == Layout.NULL ? layout : GetCurrentCardsLayout());
        difficultyLevel = GetCurrentDifficulty();

        if (cards != null)
        {
            foreach (GameObject go in cards)
                Destroy(go);
            cards.Clear();

            foreach (GameObject go in selectedCards)
                Destroy(go);
            selectedCards.Clear();

            allSeen = false;
            allSelected = false;

            scanTime = 0f;
            selectTime = 0f;       
        }

        cards = GenerateCards();
        SetCardsPositions(cards, layout);

        LeftControllerText.text = "Start";
        RightControllerText.text = "Start";
    }

    // Show pattern (after clicking Start button)
    private void ShowPattern() {
        gameState = GameState.ShowPattern;
        showingPattern = true;
        // start timer
        startCount = true;

        // flip to the front
        foreach (GameObject card in cards) {
            if (IsCardFilled(card))
                SetCardsColor(card.transform, Color.white);
            StartCoroutine(Rotate(card.transform, new Vector3(0, 180, 0), 0.5f));
        }
    }

    // Hide pattern 
    private void HidePattern(bool fromFailedTrial) {
        showingPattern = false;

        // reset timer and other variables
        startCount = false;
        
        PrintTextToScreen(SeenCardText, "");
        PrintTextToScreen(SelectCardText, "");
        PrintTextToScreen(MemoryTypeText, "");
        PrintTextToScreen(TimerText, "");

        if (!fromFailedTrial) {
            LeftControllerText.text = "Finish";
            RightControllerText.text = "Finish";

            // flip to the back
            foreach (GameObject card in cards)
            {
                if (IsCardFilled(card))
                    SetCardsColor(card.transform, Color.black);
                StartCoroutine(Rotate(card.transform, new Vector3(0, 180, 0), 0.5f));
            }

            // move to next state
            gameState = GameState.SelectCards;
            // enable the interactable feature
            foreach (GameObject card in cards)
            {
                card.GetComponent<VRTK_InteractableObject>().enabled = true;
            }
        }

    }

    // Check Result (after clicking Finish Button)
    private void FinishAnswering() {
        gameState = GameState.Result;
        CheckResult();

        // Write to Log
        if (writerAnswer != null)
        {
            writerAnswer.WriteLine(StartSceneScript.ParticipantID + "," + GetTrialNumber() + "," + GetTrialID() + "," + GetLayout() + "," +
                GetDifficulty() + "," + GetAccuracy() + "," + GetSeenTime() + "," + GetSelectTime());
            writerAnswer.Flush();
        }

        // increase trial No
        trialNo++;

        if (trialNo > maxTrialNo) {
            if (writer != null)
                writer.Close();
            if (writerHead != null)
                writerHead.Close();
            if (writerAnswer != null)
                writerAnswer.Close();
            QuitGame();
        }
    }


    ///////////////////////////////////////////////////////////////////////
    /// Game Logic
    ///////////////////////////////////////////////////////////////////////

    // check card interaction with pointer
    private void GameInteraction()
    {
        GameObject selectedCard = null;

        // assign left and right controllers interaction use
        if (mainHandIU == null) {
            if (mainHand == 0 && GameObject.Find("LeftControllerAlias") != null)
                mainHandIU = GameObject.Find("LeftControllerAlias").GetComponent<VRTK_InteractUse>();
            else if (mainHand == 1 && GameObject.Find("RightControllerAlias") != null)
                mainHandIU = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractUse>();
        }

        if (mainHandIU.GetUsingObject() != null && !IsCardRotating(mainHandIU.GetUsingObject()))
        {
            selectedCard = mainHandIU.GetUsingObject();
            if (!IsCardFlipped(selectedCard) && selectedCards.Count < difficultyLevel) // not flipped
            {
                selectedCards.Add(selectedCard);
                selectedCard.GetComponent<Card>().flipped = true;
                StartCoroutine(Rotate(selectedCard.transform, new Vector3(0, 180, 0), 0.5f));
                SetCardsColor(selectedCard.transform, Color.white);
            }
        }
    }

    // check the result
    private bool CheckResult() {
        bool finalResult = true;
        int correctNum = 0;

        if (selectedCards.Count != difficultyLevel) {
            finalResult = false;
        }
        else {
            foreach (GameObject selectedCard in selectedCards)
            {
                if (!IsCardFilled(selectedCard))
                {
                    finalResult = false;
                }
                else {
                    correctNum++;
                }
            }
        }

        accurateNumber = correctNum;

        string resultStr = (accurateNumber == difficultyLevel ? "Correct" : "Wrong");
        Debug.Log(resultStr + "! " + accurateNumber + "/" + difficultyLevel);
        
        return finalResult;
    }

    // Get current cards layouts based on sequence
    private Layout GetCurrentCardsLayout() {
        int currentTrialNo = trialNo - 1;

        switch (experimentSequence) {
            case 1:
                if (currentTrialNo % 24 <= 5 && currentTrialNo % 24 >= 0)
                    return Layout.Flat;
                else if (currentTrialNo % 24 <= 11 && currentTrialNo % 24 >= 6)
                    return Layout.LimitedFlat;
                else if (currentTrialNo % 24 <= 17 && currentTrialNo % 24 >= 12)
                    return Layout.FullCircle;
                else
                    return Layout.LimitedFullCircle;
            case 2:
                if (currentTrialNo % 24 <= 5 && currentTrialNo % 24 >= 0)
                    return Layout.LimitedFlat;
                else if (currentTrialNo % 24 <= 11 && currentTrialNo % 24 >= 6)
                    return Layout.Flat;
                else if (currentTrialNo % 24 <= 17 && currentTrialNo % 24 >= 12)
                    return Layout.LimitedFullCircle;
                else
                    return Layout.FullCircle;
            case 3:
                if (currentTrialNo % 24 <= 5 && currentTrialNo % 24 >= 0)
                    return Layout.FullCircle;
                else if (currentTrialNo % 24 <= 11 && currentTrialNo % 24 >= 6)
                    return Layout.LimitedFullCircle;
                else if (currentTrialNo % 24 <= 17 && currentTrialNo % 24 >= 12)
                    return Layout.Flat;
                else
                    return Layout.LimitedFlat;
            case 4:
                if (currentTrialNo % 24 <= 5 && currentTrialNo % 24 >= 0)
                    return Layout.LimitedFullCircle;
                else if (currentTrialNo % 24 <= 11 && currentTrialNo % 24 >= 6)
                    return Layout.FullCircle;
                else if (currentTrialNo % 24 <= 17 && currentTrialNo % 24 >= 12)
                    return Layout.LimitedFlat;
                else
                    return Layout.Flat;
            default:
                return Layout.NULL;
        }
    }


    private int GetCurrentDifficulty()
    {
        int tmp2 = (trialNo - 1) % 6;
        if (tmp2 == 0 || tmp2 == 1 || tmp2 == 2 || tmp2 == 3)
            return 3;
        else
            return 5;
    }


    // Change Layout
    private void Changelayout()
    {
        switch (layout)
        {
            case Layout.Flat:
                layout = Layout.LimitedFlat;
                break;
            case Layout.LimitedFlat:
                layout = Layout.FullCircle;
                break;
            case Layout.FullCircle:
                layout = Layout.LimitedFullCircle;
                break;
            case Layout.LimitedFullCircle:
                layout = Layout.Flat;
                break;
            default:
                break;
        }
        PrepareExperiment();
    }

    public void Changelayout(int index)
    {
        switch (index)
        {
            case 0:
                layout = Layout.LimitedFlat;
                break;
            case 1:
                layout = Layout.FullCircle;
                break;
            case 2:
                layout = Layout.LimitedFullCircle;
                break;
            case 3:
                layout = Layout.Flat;
                break;
            default:
                break;
        }
        PrepareExperiment();
    }

    // Generate Cards
    private List<GameObject> GenerateCards()
    {
        List<GameObject> cards = new List<GameObject>();
        int k = 0;

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                // calculate index number
                int index = i * numberOfColumns + j;

                // generate card game object
                string name = "Card" + index;
                GameObject card = (GameObject)Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                card.name = name;
                card.transform.parent = transform;
                card.transform.localScale = new Vector3(cardSize, cardSize, 1);

                // assign position
                card.transform.localPosition = SetCardPosition(index, i, j);

                // assign orientation
                card.transform.localEulerAngles = new Vector3(0, card.transform.localEulerAngles.y, 0);
                if (layout == Layout.FullCircle || layout == Layout.LimitedFullCircle)
                {
                    GameObject center = new GameObject();
                    center.transform.SetParent(transform);
                    center.transform.localPosition = card.transform.localPosition;
                    center.transform.localPosition = new Vector3(0, center.transform.localPosition.y, 0);

                    card.transform.LookAt(center.transform.position);

                    card.transform.localEulerAngles += Vector3.up * 180;
                    Destroy(center);
                }
                cards.Add(card);

                k++;
            }
        }
        

        int[] currentPattern = GetCurrentPattern();

        if (currentPattern != null)
        {
            for (int i = 0; i < currentPattern.Length; i++)
            {
                cards[currentPattern[i] - 1].GetComponent<Card>().filled = true;
            }
        }
        else {
            Debug.LogError("Pattern Used Up!!");
        }

        return cards;
    }

    // Set Cards Positions based on current layout
    private void SetCardsPositions(List<GameObject> localCards, Layout localLayout)
    {
        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < numberOfColumns; j++)
            {
                int index = i * numberOfColumns + j;
                localCards[index].transform.localPosition = SetCardPosition(index, i, j);

                localCards[index].transform.localEulerAngles = new Vector3(0, localCards[index].transform.localEulerAngles.y, 0);

                if (localLayout != Layout.FullCircle && localLayout != Layout.LimitedFullCircle)
                {
                    localCards[index].transform.localEulerAngles = new Vector3(0, 0, 0);
                }
                else // FULL CIRCLE
                {
                    // change orientation
                    GameObject center = new GameObject();
                    center.transform.SetParent(this.transform);
                    center.transform.localPosition = localCards[index].transform.localPosition;
                    center.transform.localPosition = new Vector3(0, center.transform.localPosition.y, 0);

                    localCards[index].transform.LookAt(center.transform.position);

                    localCards[index].transform.localEulerAngles += Vector3.up * 180;
                    Destroy(center);
                }
            }
        }

        switch (localLayout) {
            case Layout.Flat:
                transform.localPosition = new Vector3(0, adjustedHeight, -1);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                FilterCube.gameObject.SetActive(false);
                EdgeIndicator.gameObject.SetActive(false);
                break;
            case Layout.LimitedFlat:
                transform.localPosition = new Vector3(0, adjustedHeight, -1);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                FilterCube.gameObject.SetActive(true);
                EdgeIndicator.gameObject.SetActive(false);
                break;
            case Layout.FullCircle:
                transform.localPosition = new Vector3(0, adjustedHeight, 0);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                FilterCube.gameObject.SetActive(false);
                EdgeIndicator.gameObject.SetActive(true);
                break;
            case Layout.LimitedFullCircle:
                transform.localPosition = new Vector3(0, adjustedHeight, 0);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                FilterCube.gameObject.SetActive(true);
                EdgeIndicator.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    // Set Card Position
    private Vector3 SetCardPosition(int index, int row, int col)
    {
        float xValue = 0;
        float yValue = 0;
        float zValue = 0;

        switch (layout)
        {
            case Layout.Flat:
                xValue = (index - (row * numberOfColumns) - (numberOfColumns / 2.0f - 0.5f)) * hDelta;
                yValue = (numberOfRows - (row + 1)) * vDelta;
                zValue = 2;
                break;
            case Layout.LimitedFlat:
                xValue = (index - (row * numberOfColumns) - (numberOfColumns / 2.0f - 0.5f)) * hDelta;
                yValue = (numberOfRows - (row + 1)) * vDelta;
                zValue = 2;
                break;
            case Layout.FullCircle:
                xValue = -Mathf.Cos((index - (row * numberOfColumns)) * Mathf.PI / (numberOfColumns / 2.0f)) * ((numberOfColumns - 1) * hDelta / (2.0f * Mathf.PI));
                yValue = (numberOfRows - (row + 1)) * vDelta;
                zValue = Mathf.Sin((index - (row * numberOfColumns)) * Mathf.PI / (numberOfColumns / 2.0f)) * ((numberOfColumns - 1) * hDelta / (2.0f * Mathf.PI));
                break;
            case Layout.LimitedFullCircle:
                xValue = -Mathf.Cos((index - (row * numberOfColumns)) * Mathf.PI / (numberOfColumns / 2.0f)) * ((numberOfColumns - 1) * hDelta / (2.0f * Mathf.PI));
                yValue = (numberOfRows - (row + 1)) * vDelta;
                zValue = Mathf.Sin((index - (row * numberOfColumns)) * Mathf.PI / (numberOfColumns / 2.0f)) * ((numberOfColumns - 1) * hDelta / (2.0f * Mathf.PI));
                break;
            default:
                break;
        }

        return new Vector3(xValue, yValue, zValue);
    }

    // Set Card Color
    private void SetCardsColor(Transform t, Color color) {
        if (color == Color.white)
        {
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
        }
        else {
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        }
    }
    
    // scale the text on screen
    private void DetectJoystick() {
        if (mainController != null && mainController.GetChild(0) != null && mainController.GetChild(0).childCount > 0) {
            if (mainController.GetChild(0).GetChild(3).localEulerAngles.z > 15 && mainController.GetChild(0).GetChild(3).localEulerAngles.z < 20)
            {
                if (ScreenDashBoard.GetComponent<CanvasScaler>().matchWidthOrHeight <= 0.99f)
                    ScreenDashBoard.GetComponent<CanvasScaler>().matchWidthOrHeight += 0.01f;
            }
            else if (mainController.GetChild(0).GetChild(3).localEulerAngles.z < 345 && mainController.GetChild(0).GetChild(3).localEulerAngles.z > 300)
            {
                if (ScreenDashBoard.GetComponent<CanvasScaler>().matchWidthOrHeight >= 0.01f)
                    ScreenDashBoard.GetComponent<CanvasScaler>().matchWidthOrHeight -= 0.01f;
            }
        }
    }

    // timer function
    private void TimerAndCheckScan() {
        // timer function
        if (LocalMemoryTime >= 0 && startCount)
            LocalMemoryTime -= Time.deltaTime;
        
        PrintTextToScreen(TimerText, "Time Left: " + LocalMemoryTime.ToString("##.00") + "s");

        if (LocalMemoryTime < 3f)
            TimerText.color = Color.red;
        else
            TimerText.color = Color.green;

        PrintTextToScreen(DashBoardText, "");

        if (MemoryTypeText.text == "") {
            PrintTextToScreen(MemoryTypeText, "Please <color=red>remember</color> all the positions for " + difficultyLevel + " white cards.");
        }

        CheckFilledScanned();
        CheckEverythingSelected();

        if (LocalMemoryTime < 0.05f)
        {
            if(layout == Layout.LimitedFlat || layout == Layout.LimitedFullCircle)
                FilterCube.GetComponent<BoxCollider>().isTrigger = false;

            if (allSeen && allSelected)
                HidePattern(false);
            else
            {
                correctTrial = false;
                PrepareExperiment();
            }
        }
        else
        {
            // assign left and right controllers interaction use
            if (mainHandCE != null)
            {
                if (mainHandCE.touchpadPressed)
                {
                    localTouchpadPressed = true;
                }
                else
                {

                    if (allSeen && allSelected)
                    {
                        if (localTouchpadPressed)
                            HidePattern(false);
                    }

                    localTouchpadPressed = false;
                }
            }
        } 
    }

    // check user viewport
    private void CheckFilledScanned()
    {
        if (scanTime >= 0 && startCount && !allSeen)
            scanTime += Time.deltaTime;

        allSeen = true;

        foreach (GameObject go in cards)
        {
            if (IsCardFilled(go))
            {
                // check view port in flat and circle layout
                if (layout != Layout.LimitedFlat && layout != Layout.LimitedFullCircle)
                {
                    Vector3 wtvp = Camera.main.WorldToViewportPoint(go.transform.position);

                    if (wtvp.x < 0.7f && wtvp.x > 0.3f && wtvp.y < 0.8f && wtvp.y > 0.2f && wtvp.z > 0f)
                    {
                        if (go.transform.GetChild(0).GetComponent<Renderer>().isVisible)
                            go.GetComponent<Card>().seen = true;
                    }
                }
                else
                { // check in boxed and in view port in limited flat layout
                    FilterCube.GetComponent<BoxCollider>().isTrigger = true;
                }

                if (!go.GetComponent<Card>().seen)
                {
                    allSeen = false;
                }
            }
        }

        if (allSeen)
        {
            PrintTextToScreen(SeenCardText, "All cards have been seen (" + scanTime.ToString("#.0") + " s)");
            SeenCardText.color = Color.green;
        }
        else
        {
            PrintTextToScreen(SeenCardText, "You are missing some cards");
            SeenCardText.color = Color.red;
        }
    }

    // check user selected all filled cards
    private void CheckEverythingSelected()
    {
        if (selectTime >= 0 && startCount && !allSelected)
            selectTime += Time.deltaTime;

        allSelected = true;

        // assign left and right controllers interaction use
        if (mainHandIU == null)
        {
            if (mainHand == 0 && GameObject.Find("LeftControllerAlias") != null)
                mainHandIU = GameObject.Find("LeftControllerAlias").GetComponent<VRTK_InteractUse>();
            else if (mainHand == 1 && GameObject.Find("RightControllerAlias") != null)
                mainHandIU = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractUse>();
        }

        if (mainHandIU.GetUsingObject() != null)
        {
            GameObject selectedCard = mainHandIU.GetUsingObject();
            selectedCard.GetComponent<Card>().selected = true;
        }

        foreach (GameObject go in cards)
        {
            if (IsCardFilled(go)) {
                if (!go.GetComponent<Card>().selected) {
                    allSelected = false;
                }
            }
        }

        PrintTextToScreen(SelectCardText, "");

        if (allSelected)
        {
            PrintTextToScreen(SelectCardText, "All cards have been selected (" + selectTime.ToString("#.0") + " s)");
            SelectCardText.color = Color.green;
        }
        else
        {
            PrintTextToScreen(SelectCardText, "Please select all white cards");
            SelectCardText.color = Color.red;
        }

    }

    private void PrintTextToScreen(Text textBoard, string text) {
          textBoard.text = text;
    }

    // rotate coroutine with animation
    private IEnumerator Rotate(Transform rotateObject, Vector3 angles, float duration)
    {
        if (rotateObject != null)
        {
            rotateObject.GetComponent<Card>().rotating = true;
            rotateObject.GetComponent<VRTK_InteractableObject>().isUsable = false;
            Quaternion startRotation = rotateObject.rotation;
            Quaternion endRotation = Quaternion.Euler(angles) * startRotation;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                rotateObject.rotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
                yield return null;
            }
            rotateObject.rotation = endRotation;
            rotateObject.GetComponent<VRTK_InteractableObject>().isUsable = true;

            rotateObject.GetComponent<Card>().rotating = false;
        }
    }

    private int[] GetCurrentPattern() {
        if (difficultyLevel == 3)
        {
            if (LvL3TaskList.Count > 0)
            {
                int[] PatternID = new int[difficultyLevel];
                string[] PatternIDString = new string[difficultyLevel];

                PatternIDString = LvL3TaskList[0].Split(fieldSeperator);

                LvL3TaskList.RemoveAt(0);

                for (int i = 0; i < 6; i++)
                {
                    PatternID[i] = int.Parse(PatternIDString[i]);
                }
                return PatternID;
            }
        }
        else if (difficultyLevel == 5) {
            if (LvL5TaskList.Count > 0)
            {
                int[] PatternID = new int[difficultyLevel];
                string[] PatternIDString = new string[difficultyLevel];

                PatternIDString = LvL5TaskList[0].Split(fieldSeperator);

                LvL5TaskList.RemoveAt(0);

                for (int i = 0; i < 6; i++)
                {
                    PatternID[i] = int.Parse(PatternIDString[i]);
                }
                return PatternID;
            }
        }
        return null;
    }


    private void ReadPatternsFromInput() {
        string[] lines = new string[40];

        lines = Patterns3.text.Split(lineSeperater);

        LvL3TaskList.AddRange(lines);

        // shuffle order for lists
        for (int i = 0; i < LvL3TaskList.Count; i++)
        {
            string temp = LvL3TaskList[i];
            int randomIndex = Random.Range(i, LvL3TaskList.Count);
            LvL3TaskList[i] = LvL3TaskList[randomIndex];
            LvL3TaskList[randomIndex] = temp;
        }

        lines = new string[40];

        lines = Patterns5.text.Split(lineSeperater);

        LvL5TaskList.AddRange(lines);

        // shuffle order for lists
        for (int i = 0; i < LvL5TaskList.Count; i++)
        {
            string temp = LvL5TaskList[i];
            int randomIndex = Random.Range(i, LvL5TaskList.Count);
            LvL5TaskList[i] = LvL5TaskList[randomIndex];
            LvL5TaskList[randomIndex] = temp;
        }
    }

    /// <summary>
    /// Log related functions
    /// </summary>

    float GetFixedTime()
    {
        float finalTime = 0;
        if (StartSceneScript.lastTimePast != 0)
        {
            finalTime = StartSceneScript.lastTimePast + Time.fixedTime;
        }
        else
        {
            finalTime = Time.fixedTime;
        }
        return finalTime;
    }

    private string GetUserID()
    {
        return StartSceneScript.ParticipantID.ToString();
    }

    private string GetTrialNumber()
    {
        return trialNo.ToString();
    }

    private string GetTrialID() {
        if (trialNo % 6 == 1 || trialNo % 6 == 2)
            return "Training";
        else
            return (trialNo - (((int)((trialNo - 1) / 6)) + 1) * 2).ToString();
    }

    private string GetGameState() {
        switch (gameState) {
            case GameState.Prepare:
                return "prepare";
            case GameState.Result:
                return "result";
            case GameState.SelectCards:
                return "selectCards";
            case GameState.ShowPattern:
                return "showPattern";
            default:
                return "";
        }
    }

    private string GetLayout()
    {
        int tmp = (trialNo - 1) / 6;
        switch (tmp)
        {
            case 0:
                return "Flat";
            case 1:
                return "LimitedFlat";
            case 2:
                return "FullCircle";
            case 3:
                return "LimitedFullCircle";
            default:
                return "";
        }
    }

    private string GetDifficulty()
    {
        int tmp2 = (trialNo - 1) % 6;
        if (tmp2 == 0 || tmp2 == 1 || tmp2 == 2 || tmp2 == 3)
            return "3";
        else
            return "5";
    }

    private string GetAccuracy() {
        return accurateNumber + "/" + difficultyLevel;
    }

    private string GetSeenTime()
    {
        return scanTime.ToString("#.0") + " s";
    }

    private string GetSelectTime()
    {
        return selectTime.ToString("#.0") + " s";
    }

    // Check if card filled property is true
    private bool IsCardFilled(GameObject go)
    {
        if (go.GetComponent<Card>().filled)
            return true;
        return false;
    }

    // Check if card flipped property is true
    private bool IsCardFlipped(GameObject go)
    {
        if (go.GetComponent<Card>().flipped)
            return true;
        return false;
    }

    // Check if card flipped property is true
    private bool IsCardRotating(GameObject go)
    {
        if (go.GetComponent<Card>().rotating)
            return true;
        return false;
    }

    string VectorToString(Vector3 v)
    {
        string text;
        text = v.x + "," + v.y + "," + v.z;
        return text;
    }

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
