using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using TMPro;
using UnityEngine.UI;
using System.IO;
//using System;
using VRTK.GrabAttachMechanics;

public enum GameState
{
    Prepare,
    ShowPattern,
    Distractor,
    SelectCards,
    Result,
    Break
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
    public GameObject LimitedVisionGO;
    public Transform FlatLandmarks;
    public Transform CircularLandmarks;
    //public Text DashBoardText;
    //public Text SeenCardText;
    //public Text SelectCardText;
    //public Text MemoryTypeText;
    //public Text TimerText;
    public Text Instruction;
    public Text InstructionTimer;
    public Transform FootPrint;
    public TextMeshProUGUI LeftControllerText;
    public TextMeshProUGUI RightControllerText;
    //public Transform FilterCube;
    //public Transform EdgeIndicator;
    //public Transform Hoop;
    //public Transform Stand;
    //public Transform Ball;
    public Transform CardGame;
    public AudioClip TimesUp;
    public AudioClip wrongAnswer;

    [Header("Task File")]
    public TextAsset Patterns2;
    public TextAsset Patterns3;
    public TextAsset Patterns5;
    public TextAsset Patterns5Flat;
    public TextAsset Patterns5Circular;
    public TextAsset Patterns5Supp;
    public TextAsset GameTask;

    [Header("Predefined Variables")]
    public float hDelta;
    public float vDelta;
    public float cardSize;
    public float memoryTime;
    public float distractorTime;
    public int numberOfRows;
    public int numberOfColumns;

    [Header("Variables")]
    public Layout layout;
    public int difficultyLevel;
    public bool BlindFold;
    public bool Landmark;

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
    private VRTK_InteractTouch mainHandIT;
    private int mainHandIndex = -1;
    private VRTK_ControllerEvents mainHandCE;
    private int maxTrialNo = 20;
    // string variables
    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

    // incremental with process
    [HideInInspector]
    public GameState gameState;
    private int trialNo = 1;

    // refresh every trail
    private List<GameObject> cards;
    private List<GameObject> gameCards;
    private List<GameObject> selectedCards;
    private List<GameObject> selectedGameCards;
    //private bool correctTrial = true; // true if user answer all correct cards
    private float LocalMemoryTime;
    private float localDistractorTime;
    private List<string> LvL2TaskList;
    private List<string> LvL3TaskList;
    private List<string> LvL5TaskList;
    private List<string> LvL5FlatTaskList;
    private List<string> LvL5CircularTaskList;
    private List<string> LvL5SuppTaskList;
    private List<string> GameTaskList;
    private int[] currentPattern;
    private int[] currentGameTask;
    private int[] answerPattern;
    private int currentGameNumber; // game 2
    private float localTimer; // game 2
    private bool insideCards = false; // game2

    // refresh in one process
    private bool showingPattern = false; // show pattern stage
    private bool startCount = false; // show pattern stage
    private bool allSeen = false;
    private bool soundPlayed = false;
    [HideInInspector]
    private float scanTime = 0;
    private bool allSelected = false;
    private float selectTime = 0;
    private int accurateNumber = 0;
    private int shootCount = 0;
    //private int trainingSuppCount = 0;
    //private bool failedTraining = false;
    private int gameFinshedCount = 0;

    // check on update for interaction
    private bool localTouchpadPressed = false;
    private bool localMenuPressed = false;
    private bool instruction = true;
    private bool playgroundFlag = false;
    private int basketballScore = 0;
    

    // log use
    private string trialID;
    private int experimentSequence;
    private float currentAllSeenTime;
    private float currentAllSelectTime;
    [HideInInspector]
    public int shootTotalNumber = 0;
    [HideInInspector]
    public List<float> seenTimeLog;
    private List<float> selectTimeLog;
    StreamWriter writer;
    StreamWriter writerHead;
    StreamWriter writerAnswer;
    StreamWriter writerInteraction;
    StreamWriter writerTrialCards;
    StreamWriter writerAnswerCards;

    // Start is called before the first frame update
    void Start()
    {
        // initialise variables
        cards = new List<GameObject>();
        gameCards = new List<GameObject>();
        selectedCards = new List<GameObject>();
        selectedGameCards = new List<GameObject>();

        LvL2TaskList = new List<string>();
        LvL3TaskList = new List<string>();
        LvL5TaskList = new List<string>();
        LvL5FlatTaskList = new List<string>();
        LvL5CircularTaskList = new List<string>();
        LvL5SuppTaskList = new List<string>();
        GameTaskList = new List<string>();

        seenTimeLog = new List<float>();
        selectTimeLog = new List<float>();

        ReadPatternsFromInput();

        foreach (Transform t in CardGame) {
            gameCards.Add(t.gameObject);
        }
        

        // setup main hand
        if (StartSceneScript.controllerHand == 0 || StartSceneScript.controllerHand == 1)
            mainHand = StartSceneScript.controllerHand;
        else
            mainHand = 1;

        // setup adjusted height
        adjustedHeight = StartSceneScript.adjustedHeight;
        //Hoop.position = new Vector3(-0.5f, adjustedHeight + 1.75f, 1.29f);

        // setup experimentSequence
        experimentSequence = StartSceneScript.ExperimentSequence;

        // setup trail Number
        if (StartSceneScript.PublicTrialNumber != 0)
            trialNo = StartSceneScript.PublicTrialNumber;

        // setup writer stream
        string writerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_RawData.csv";
        writer = new StreamWriter(writerFilePath, true);

        string writerHeadFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_HeadAndHand.csv";
        writerHead = new StreamWriter(writerHeadFilePath, true);

        string writerAnswerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_Answers.csv";
        writerAnswer = new StreamWriter(writerAnswerFilePath, true);

        string writerInteractionFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/Participant_" + StartSceneScript.ParticipantID + "_Interaction.csv";
        writerInteraction = new StreamWriter(writerInteractionFilePath, true);

        string writerTrialCardsFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/trialCards.csv";
        writerTrialCards = new StreamWriter(writerTrialCardsFilePath, true);

        string writerAnswerCardsFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + StartSceneScript.ParticipantID + "/answerCards.csv";
        writerAnswerCards = new StreamWriter(writerAnswerCardsFilePath, true);

        LocalMemoryTime = memoryTime;
        localDistractorTime = distractorTime;
        localTimer = 4;
        // setup experiment
        PrepareExperiment();

       
    }

    // Update is called once per frame
    void Update()
    {
        if (layout == Layout.FullCircle)
        {
            transform.localEulerAngles = new Vector3(0, 15, 0);
            if(BlindFold)
                LimitedVisionGO.SetActive(true);
            if (Landmark) {
                if (gameState == GameState.ShowPattern || gameState == GameState.SelectCards) {
                    CircularLandmarks.position = new Vector3(0, adjustedHeight, 0);
                    CircularLandmarks.gameObject.SetActive(true);
                    FlatLandmarks.gameObject.SetActive(false);
                }  
            }
        }
        else {
            if(BlindFold)
                LimitedVisionGO.SetActive(true);
            if (Landmark) {
                if (gameState == GameState.ShowPattern || gameState == GameState.SelectCards)
                {
                    FlatLandmarks.position = new Vector3(0, adjustedHeight, 1);
                    FlatLandmarks.gameObject.SetActive(true);
                    CircularLandmarks.gameObject.SetActive(false);
                }
            }
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
            

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

        if (gameState == GameState.ShowPattern)
            TimerAndCheckScan();

        if (gameState == GameState.Distractor) {
            ShowPlayground();
            PlaygroundInteraction();
        }

        if (gameState == GameState.SelectCards)
        {
            GameInteraction();
            Instruction.transform.position = new Vector3(0, 2.5f, 1.35f);
            Instruction.text = selectedCards.Count + " / " + difficultyLevel;
        }

        CheckStateChange();

        WritingToLog();
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
                            // check position
                            if (Camera.main.transform.position.x < 0.4f && Camera.main.transform.position.x > -0.4f && Camera.main.transform.position.z < 0.4f &&
                               Camera.main.transform.position.z > -0.4f) {
                                FootPrint.gameObject.SetActive(false);
                                ShowPattern();
                            }   
                            break;
                        case GameState.ShowPattern:
                            break;
                        case GameState.Distractor:
                            break;
                        case GameState.SelectCards:
                            if (selectedCards.Count == difficultyLevel) {
                                LeftControllerText.text = "Ready";
                                RightControllerText.text = "Ready";
                                FinishAnswering();
                                Instruction.text = "Result: " + accurateNumber + " / " + difficultyLevel;
                            }
                            break;
                        case GameState.Result:
                            if (trialNo == 9 || trialNo == 15)
                            {
                                //trainingSuppCount = 0;
                                Instruction.text = "Break for three minutes.";
                                LeftControllerText.text = "Start";
                                RightControllerText.text = "Start";

                                gameState = GameState.Break;
                            }
                            else
                            {
                                LeftControllerText.text = "Start";
                                RightControllerText.text = "Start";
                                //if (failedTraining)
                                //    PlaySuppTrial();
                                //else
                                PrepareExperiment();
                            }
                            break;
                        case GameState.Break:
                            PrepareExperiment();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    //private void PlaySuppTrial() {


    //    trainingSuppCount++;

    //    gameState = GameState.Prepare;
    //    LocalMemoryTime = memoryTime;
    //    localDistractorTime = distractorTime;

    //    WriteInteractionToLog("Prepare");

    //    if (GetTrialID() == "Training")
    //        Instruction.text = "Training Task: " + ((trialNo % 10) + trainingSuppCount);
    //    else
    //        Instruction.text = "Experiment Task: " + ((trialNo - 2) % 10) + " / 8";
    //    FootPrint.gameObject.SetActive(true);

    //    if (cards != null)
    //    {
    //        foreach (GameObject go in cards)
    //            Destroy(go);
    //        cards.Clear();

    //        foreach (GameObject go in selectedCards)
    //            Destroy(go);
    //        selectedCards.Clear();
    //    }

    //    allSeen = false;
    //    allSelected = false;
    //    soundPlayed = false;

    //    scanTime = 0f;
    //    selectTime = 0f;
    //    shootCount = 0;

    //    seenTimeLog.Clear();
    //    selectTimeLog.Clear();

    //    cards = GenerateCards();

    //    failedTraining = false;

    //    SetCardsPositions(cards, layout);

    //    LeftControllerText.text = "Start";
    //    RightControllerText.text = "Start";

    //    foreach (GameObject card in cards)
    //    {
    //        card.SetActive(false);
    //    }
    //    EdgeIndicator.gameObject.SetActive(false);
    //}

    // Prepare stage (after clicking ready button)
    private void PrepareExperiment() {
        gameState = GameState.Prepare;
        LocalMemoryTime = memoryTime;
        localDistractorTime = distractorTime;
        localTimer = 4;

        // change trial conditions based on trial number
        // layout
        if (GetCurrentCardsLayout() != Layout.NULL)
            layout = GetCurrentCardsLayout();
        difficultyLevel = GetCurrentDifficulty();

        //if (correctTrial)
            WriteInteractionToLog("Prepare");
        //else
        //    WriteInteractionToLog("FailedTrialPreparation");

        //if (correctTrial)
        //{
            if (GetTrialID() == "Training")
                Instruction.text = "Training Task: " + trialNo;
            else
                Instruction.text = "Experiment Task: " + (trialNo - 2) + " / 18";
            FootPrint.gameObject.SetActive(true);
        //}
        //else
        //{
        //    HidePattern(true);
        //    if (!allSeen && allSelected)
        //        Instruction.text = "You haven't seen all the cards. Please restart.";
        //    else if (!allSelected && allSeen)
        //        Instruction.text = "You haven't selected all the cards. Please restart.";
        //    else if (!allSelected && !allSeen)
        //        Instruction.text = "You haven't seen and selected all the cards. Please restart.";
        //}

        //correctTrial = true;

        if (cards != null)
        {
            foreach (GameObject go in cards)
                Destroy(go);
            cards.Clear();

            foreach (GameObject go in selectedCards)
                Destroy(go);
            selectedCards.Clear();
        }

        allSeen = false;
        allSelected = false;
        soundPlayed = false;

        scanTime = 0f;
        selectTime = 0f;
        shootCount = 0;
        gameFinshedCount = 0;

        seenTimeLog.Clear();
        selectTimeLog.Clear();

        cards = GenerateCards();

        SetCardsPositions(cards, layout);

        LeftControllerText.text = "Start";
        RightControllerText.text = "Start";

        foreach (GameObject card in cards)
        {
            card.SetActive(false);
        }
        //EdgeIndicator.gameObject.SetActive(false);
    }

    // Show pattern (after clicking Start button)
    private void ShowPattern() {
        Instruction.text = "";
        WriteInteractionToLog("ShowPattern");
        foreach (GameObject card in cards)
        {
            card.SetActive(true);
        }
        //if (layout == Layout.FullCircle || layout == Layout.LimitedFullCircle)
            //EdgeIndicator.gameObject.SetActive(true);

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

        if (!fromFailedTrial) {
            if (gameState == GameState.ShowPattern) {
                gameState = GameState.Distractor;
                WriteInteractionToLog("Distractor");
            }
            else {
                LeftControllerText.text = "Finish";
                RightControllerText.text = "Finish";

                // flip to the back
                foreach (GameObject card in cards)
                {
                    if (IsCardFilled(card))
                        SetCardsColor(card.transform, Color.black);
                    StartCoroutine(Rotate(card.transform, new Vector3(0, 180, 0), 0.5f));
                }
                WriteInteractionToLog("SelectedCards");
                // move to next state
                gameState = GameState.SelectCards;
                // enable the interactable feature
                foreach (GameObject card in cards)
                {
                    card.GetComponent<VRTK_InteractableObject>().enabled = true;
                }
            }
        }
    }

    // check the result
    private bool CheckResult()
    {
        bool finalResult = true;
        int correctNum = 0;

        if (selectedCards.Count != difficultyLevel)
            finalResult = false;
        else
        {
            foreach (GameObject selectedCard in selectedCards)
            {
                if (!IsCardFilled(selectedCard))
                    finalResult = false;
                else
                    correctNum++;
            }
        }

        accurateNumber = correctNum;

        string resultStr = (accurateNumber == difficultyLevel ? "Correct" : "Wrong");
        Debug.Log(resultStr + "! " + accurateNumber + "/" + difficultyLevel);

        return finalResult;
    }

    // Check Result (after clicking Finish Button)
    private void FinishAnswering() {
        gameState = GameState.Result;
        foreach (GameObject card in cards)
        {
            card.SetActive(false);
        }

        WriteInteractionToLog("Result");
        CheckResult();

        // increase trial No
        if (GetTrialID() == "Training")
        {
        //    if (accurateNumber != difficultyLevel) {
        //        failedTraining = true;
        //        trialNo--;
        //    }
        }
        else {
            // Write cards log for accuracy
            WriteCardsLog();
            // Write to Log
            WriteAnswerToLog();
        }
            trialNo++;

        if (trialNo > maxTrialNo) {
            if (writer != null) {
                writer.Close();
                writer = null;
            }
            
            if (writerHead != null)
            {
                writerHead.Close();
                writerHead = null;
            }
            
            if (writerAnswer != null)
            {
                writerAnswer.Close();
                writerAnswer = null;
            }
            
            if (writerInteraction != null)
            {
                writerInteraction.Close();
                writerInteraction = null;
            }
            
            if (writerTrialCards != null)
            {
                writerTrialCards.Close();
                writerTrialCards = null;
            }
            
            if (writerAnswerCards != null)
            {
                writerAnswerCards.Close();
                writerAnswerCards = null;
            }

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

        // assign left and right controllers interaction touch
        if (mainHandIT == null)
            mainHandIT = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractTouch>();

        // assign main hand index
        if (mainHandIndex == -1)
            mainHandIndex = (int)GameObject.Find("Controller (right)").GetComponent<VRTK_TrackedController>().index;

        if (mainHandIT.GetTouchedObject() != null) {
            // haptic function
            SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);

            if (!IsCardRotating(mainHandIT.GetTouchedObject()))
            {
                selectedCard = mainHandIT.GetTouchedObject();
                if (!IsCardFlipped(selectedCard) && selectedCards.Count < difficultyLevel) // not flipped
                {
                    WriteInteractionToLog(selectedCard.name + " answered");
                    selectedCards.Add(selectedCard);
                    selectedCard.GetComponent<Card>().flipped = true;
                    StartCoroutine(Rotate(selectedCard.transform, new Vector3(0, 180, 0), 0.5f));
                    SetCardsColor(selectedCard.transform, Color.white);
                }
            }
        }

        /// (OLD) click to select cards
        // assign left and right controllers interaction use
        //if (mainHandIU == null) {
        //    if (mainHand == 0 && GameObject.Find("LeftControllerAlias") != null)
        //        mainHandIU = GameObject.Find("LeftControllerAlias").GetComponent<VRTK_InteractUse>();
        //    else if (mainHand == 1 && GameObject.Find("RightControllerAlias") != null)
        //        mainHandIU = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractUse>();
        //}

        //if (mainHandIU.GetUsingObject() != null && !IsCardRotating(mainHandIU.GetUsingObject()))
        //{
        //    selectedCard = mainHandIU.GetUsingObject();
        //    if (!IsCardFlipped(selectedCard) && selectedCards.Count < difficultyLevel) // not flipped
        //    {
        //        selectedCards.Add(selectedCard);
        //        selectedCard.GetComponent<Card>().flipped = true;
        //        StartCoroutine(Rotate(selectedCard.transform, new Vector3(0, 180, 0), 0.5f));
        //        SetCardsColor(selectedCard.transform, Color.white);
        //    }
        //}
    }

    private void ShowPlayground() {
        if (!playgroundFlag) {

            // reset border color
            foreach (GameObject go in cards)
            {
                go.GetComponent<Card>().seen = false;
                go.GetComponent<Card>().seenLogged = false;
                go.GetComponent<Card>().selected = false;
                go.GetComponent<Card>().selectLogged = false;
                go.GetComponent<Card>().ResetBorderColor();
            }

            playgroundFlag = true;
            //PrintTextToScreen(DashBoardText, "Please return to the original position and face to the front.");
            FootPrint.gameObject.SetActive(true);

            // hide cards
            foreach (GameObject card in cards) {
                card.SetActive(false);
            }
            //if (layout == Layout.FullCircle || layout == Layout.LimitedFullCircle)
                //EdgeIndicator.gameObject.SetActive(false);
            //if (layout == Layout.LimitedFlat || layout == Layout.LimitedFullCircle)
            //    FilterCube.gameObject.SetActive(false);

            // check position
            if (Camera.main.transform.position.x < 0.4f && Camera.main.transform.position.x > -0.4f && Camera.main.transform.position.z < 0.4f &&
               Camera.main.transform.position.z > -0.4f)
            {
                currentGameTask = GetCurrentGameTask();
                
                soundPlayed = false;
                //// show hoop and ball
                //Hoop.gameObject.SetActive(true);
                //Stand.gameObject.SetActive(true);
                //Ball.gameObject.SetActive(true);
                if (StartSceneScript.distratorType == 0)
                {
                    Instruction.text = string.Join(", ", currentGameTask);
                    WriteInteractionToLog("Task: " + string.Join("; ", currentGameTask));
                    CardGame.gameObject.SetActive(false);
                }
                else {
                    CardGame.gameObject.SetActive(true);
                    int task = RandomNumber(1, 9);
                    currentGameNumber = task;
                    Instruction.text = task + "";
                    WriteInteractionToLog("Task: " + task);

                    Instruction.transform.parent.parent.position = new Vector3(0, CardGame.position.y + 1.4f, 0.5f);
                }

                FootPrint.gameObject.SetActive(false);
                ////PrintTextToScreen(DashBoardText, "Play Basketball game in 15 seconds. Try to score as many as you can.");
                //GameObject.Find("Hoop").GetComponent<Basket>().ResetScore();
            }
            else
                playgroundFlag = false;
        }
    }

    private void HidePlayground() {
        if (playgroundFlag) {
            playgroundFlag = false;
            //PrintTextToScreen(DashBoardText, "Please return to the original position and face to the front.");
            FootPrint.gameObject.SetActive(true);
            Instruction.text = "";

            foreach (GameObject go in gameCards)
            {
                go.GetComponent<Card>().filled = false;
                go.GetComponent<Card>().seen = false;
                go.GetComponent<Card>().selected = false;
                go.GetComponent<Card>().ResetBorderColor();
            }
            selectedGameCards.Clear();
            CardGame.gameObject.SetActive(false);

            //if (Hoop.gameObject.activeSelf)
            //{
            //    // hide hoop and ball
            //    Hoop.gameObject.SetActive(false);
            //}

            //if (Stand.gameObject.activeSelf)
            //{
            //    Stand.gameObject.SetActive(false);
            //}

            //// reset ball position
            //if (Ball.gameObject.activeSelf) {
            //    Ball.transform.position = new Vector3(0, 1.144f, 0.5f);
            //    Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //    Ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //    Ball.gameObject.SetActive(false);
            //}

            // check position
            if (Camera.main.transform.position.x < 0.4f && Camera.main.transform.position.x > -0.4f && Camera.main.transform.position.z < 0.4f &&
               Camera.main.transform.position.z > -0.4f)
            {
                InstructionTimer.text = "";
                FootPrint.gameObject.SetActive(false);
                // show cards
                foreach (GameObject card in cards)
                {
                    card.SetActive(true);
                }
                //if (layout == Layout.FullCircle || layout == Layout.LimitedFullCircle)
                    //EdgeIndicator.gameObject.SetActive(true);
                //if (layout == Layout.LimitedFlat || layout == Layout.LimitedFullCircle)
                //    FilterCube.gameObject.SetActive(true);

                Instruction.transform.parent.parent.position = new Vector3(0, 2.5f, 1.35f);

                soundPlayed = false;
                HidePattern(false);
            }
            else
                playgroundFlag = true;
        }
    }

    private void PlaygroundInteraction() {
        // timer function
        if (playgroundFlag) {
            if (localDistractorTime >= 0)
                localDistractorTime -= Time.deltaTime;
            if(localTimer >= 0)
                localTimer -= Time.deltaTime;
        }

        InstructionTimer.text = localDistractorTime.ToString("0.0");

        if (StartSceneScript.distratorType == 0) // 5 number sequence tapping
        {
            if (mainHandCE.touchpadPressed)
                localTouchpadPressed = true;
            if (!mainHandCE.touchpadPressed && localTouchpadPressed)
            {
                localTouchpadPressed = false;
                if (Instruction.text == "" && selectedGameCards.Count == 5)
                {
                    CheckGameResult();
                }
                else
                {
                    Instruction.text = "";
                    CardGame.gameObject.SetActive(true);
                }
            }

            //CardGame.gameObject.SetActive(true);

            GameObject selectedCard = null;
            if (mainHandIT.GetTouchedObject() != null)
            {
                // haptic function
                SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);
                if (selectedGameCards.Count < 5)
                {
                    selectedCard = mainHandIT.GetTouchedObject();
                    if (!selectedGameCards.Contains(selectedCard))
                    {
                        selectedGameCards.Add(selectedCard);
                        WriteInteractionToLog(selectedCard.name + " played");
                    }


                    selectedCard.GetComponent<Card>().filled = true;
                    selectedCard.GetComponent<Card>().seen = true;
                    selectedCard.GetComponent<Card>().selected = true;
                }
            }

            if (localDistractorTime < 3.5f)
            {
                if (!soundPlayed)
                {
                    AudioSource.PlayClipAtPoint(TimesUp, transform.position);
                    soundPlayed = true;
                }
            }

            // times up or finish one set, who comes second
            if (localDistractorTime < 0.05f && gameFinshedCount > 0)
                HidePlayground();
        }
        else
        { // 1 number continuing tapping

            //if (mainHandCE.touchpadPressed)
            //    localTouchpadPressed = true;
            //if (!mainHandCE.touchpadPressed && localTouchpadPressed)
            //{
            //    localTouchpadPressed = false;
            //    if (Instruction.text == "" && selectedGameCards.Count == 5)
            //    {
            //        CheckGameResult();
            //    }
            //    else
            //    {
            //        Instruction.text = "";
            //        CardGame.gameObject.SetActive(true);
            //    }
            //}

            //CardGame.gameObject.SetActive(true);

            GameObject selectedCard = null;

            if (mainHandIT.GetTouchedObject() != null && !insideCards)
            {
                // haptic function
                SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);
                if (selectedGameCards.Count == 0)
                {
                    selectedCard = mainHandIT.GetTouchedObject();
                    if (!selectedGameCards.Contains(selectedCard))
                    {
                        selectedGameCards.Add(selectedCard);
                        WriteInteractionToLog(selectedCard.name + " played");
                    }

                    selectedCard.GetComponent<Card>().filled = true;
                    selectedCard.GetComponent<Card>().seen = true;
                    selectedCard.GetComponent<Card>().selected = true;
                    insideCards = true;
                }
            }

            if ((mainHandCE.transform.parent.position.x > 0.85f || mainHandCE.transform.parent.position.x < -0.85f) ||
                (mainHandCE.transform.parent.position.y > 2.36f || mainHandCE.transform.parent.position.y < 0.63f) ||
                (mainHandCE.transform.parent.position.z > 0.67f || mainHandCE.transform.parent.position.z < 0.31f)) {
                insideCards = false;
            }

            if (selectedGameCards.Count == 1) {
                if (selectedCard.name != currentGameNumber.ToString())
                {
                    localDistractorTime += 4;
                    // play sound or show text
                    //AudioSource.PlayClipAtPoint(wrongAnswer, Camera.main.transform.position);
                    InstructionTimer.color = Color.red;
                }
                else
                    InstructionTimer.color = Color.white;
                ResetCardGame();
            } else if (localTimer <= 0) {
                InstructionTimer.color = Color.red;
                localDistractorTime += 4;
                // play sound or show text
                //AudioSource.PlayClipAtPoint(wrongAnswer, Camera.main.transform.position);
                ResetCardGame();
            }

            if (localDistractorTime < 3.5f)
            {
                if (!soundPlayed)
                {
                    AudioSource.PlayClipAtPoint(TimesUp, transform.position);
                    soundPlayed = true;
                }
            }

            // times up or finish one set, who comes second
            if (localDistractorTime < 0.05f)
                HidePlayground();
        }
       
        //Instruction.transform.position = new Vector3(0, 1.5f, 1.35f);
        //Instruction.text = "Score: " + shootTotalNumber + "\n";

        
        
    }

    private void CheckGameResult() {
        bool allCorrect = true;

        for (int i = 0; i < selectedGameCards.Count; i++) {
            if (selectedGameCards[i].name != currentGameTask[i].ToString())
                allCorrect = false;
        }

        if (allCorrect) {
            gameFinshedCount++;
        }

        ResetCardGame();
    }

    private void ResetCardGame() {
        localTimer = 4;

        // reset border color
        foreach (GameObject go in gameCards)
        {
            go.GetComponent<Card>().filled = false;
            go.GetComponent<Card>().seen = false;
            go.GetComponent<Card>().selected = false;
            go.GetComponent<Card>().ResetBorderColor();
        }

        // shuffle order for cards
        for (int i = 0; i < CardGame.childCount; i++)
        {
            Vector3 temp = CardGame.GetChild(i).localPosition;
            int randomIndex = Random.Range(i, CardGame.childCount);
            CardGame.GetChild(i).localPosition = CardGame.GetChild(randomIndex).localPosition;
            CardGame.GetChild(randomIndex).localPosition = temp;
        }

        selectedGameCards.Clear();

        if (StartSceneScript.distratorType == 0)
        {
            currentGameTask = GetCurrentGameTask();
            CardGame.gameObject.SetActive(false);

            Instruction.text = string.Join(", ", currentGameTask);
            WriteInteractionToLog("Task: " + string.Join(";", currentGameTask));
        }
        else {
            int task = RandomNumber(1, 9);
            currentGameNumber = task;

            Instruction.text = task + "";
            WriteInteractionToLog("Task: " + task);
        }
    }


    // Get current cards layouts based on sequence
    private Layout GetCurrentCardsLayout() {
        //int currentTrialNo = trialNo - 1;

        switch (experimentSequence)
        {
            case 1:
                if (trialNo % 2 == 1)
                    return Layout.Flat;
                else if(trialNo <= 20)
                    return Layout.FullCircle;
                else
                    return Layout.LimitedFlat;
            case 2:
                if (trialNo % 2 == 1)
                    return Layout.FullCircle;
                else
                    return Layout.Flat;
            default:
                return Layout.NULL;
        }

        //switch (experimentSequence) {
        //    case 1:
        //        if (currentTrialNo % 28 <= 6 && currentTrialNo % 28 >= 0)
        //            return Layout.Flat;
        //        else if (currentTrialNo % 28 <= 13 && currentTrialNo % 28 >= 7)
        //            return Layout.LimitedFlat;
        //        else if (currentTrialNo % 28 <= 20 && currentTrialNo % 28 >= 14)
        //            return Layout.FullCircle;
        //        else
        //            return Layout.LimitedFullCircle;
        //    case 2:
        //        if (currentTrialNo % 28 <= 6 && currentTrialNo % 28 >= 0)
        //            return Layout.LimitedFlat;
        //        else if (currentTrialNo % 28 <= 13 && currentTrialNo % 28 >= 7)
        //            return Layout.Flat;
        //        else if (currentTrialNo % 28 <= 20 && currentTrialNo % 28 >= 14)
        //            return Layout.LimitedFullCircle;
        //        else
        //            return Layout.FullCircle;
        //    case 3:
        //        if (currentTrialNo % 28 <= 6 && currentTrialNo % 28 >= 0)
        //            return Layout.FullCircle;
        //        else if (currentTrialNo % 28 <= 13 && currentTrialNo % 28 >= 7)
        //            return Layout.LimitedFullCircle;
        //        else if (currentTrialNo % 28 <= 20 && currentTrialNo % 28 >= 14)
        //            return Layout.Flat;
        //        else
        //            return Layout.LimitedFlat;
        //    case 4:
        //        if (currentTrialNo % 28 <= 6 && currentTrialNo % 28 >= 0)
        //            return Layout.LimitedFullCircle;
        //        else if (currentTrialNo % 28 <= 13 && currentTrialNo % 28 >= 7)
        //            return Layout.FullCircle;
        //        else if (currentTrialNo % 28 <= 20 && currentTrialNo % 28 >= 14)
        //            return Layout.LimitedFlat;
        //        else
        //            return Layout.Flat;
        //    default:
        //        return Layout.NULL;
        //}
    }


    private int GetCurrentDifficulty()
    {
        //int tmp = (trialNo - 1) % 7;
        //if (tmp == 0)
        //    return 2;
        //else if (tmp == 1 || tmp == 2 || tmp == 3)
        //    return 3;
        //else
        return 5;
    }

    private int[] GetCurrentGameTask() {
        if (GameTaskList.Count > 0)
        {
            int[] PatternID = new int[difficultyLevel];
            string[] PatternIDString = new string[difficultyLevel];

            PatternIDString = GameTaskList[0].Split(fieldSeperator);

            GameTaskList.RemoveAt(0);

            for (int i = 0; i < difficultyLevel; i++)
            {
                PatternID[i] = int.Parse(PatternIDString[i]);
            }
            return PatternID;
        }

        return null;
    }

    // get current pattern
    private int[] GetCurrentPattern()
    {
        if (difficultyLevel == 2)
        {
            if (LvL2TaskList.Count > 0)
            {
                int[] PatternID = new int[difficultyLevel];
                string[] PatternIDString = new string[difficultyLevel];

                PatternIDString = LvL2TaskList[0].Split(fieldSeperator);

                LvL2TaskList.RemoveAt(0);

                for (int i = 0; i < difficultyLevel; i++)
                {
                    PatternID[i] = int.Parse(PatternIDString[i]);
                }
                return PatternID;
            }
        }
        else if (difficultyLevel == 3)
        {
            if (LvL3TaskList.Count > 0)
            {
                int[] PatternID = new int[difficultyLevel];
                string[] PatternIDString = new string[difficultyLevel];

                PatternIDString = LvL3TaskList[0].Split(fieldSeperator);

                LvL3TaskList.RemoveAt(0);

                for (int i = 0; i < difficultyLevel; i++)
                {
                    PatternID[i] = int.Parse(PatternIDString[i]);
                }
                return PatternID;
            }
        }
        else if (difficultyLevel == 5)
        {
            //if (LvL5TaskList.Count > 0)
            //{
            //    int[] PatternID = new int[difficultyLevel];
            //    string[] PatternIDString = new string[difficultyLevel];

            //    PatternIDString = LvL5TaskList[0].Split(fieldSeperator);

            //    LvL5TaskList.RemoveAt(0);

            //    for (int i = 0; i < difficultyLevel; i++)
            //    {
            //        PatternID[i] = int.Parse(PatternIDString[i]);
            //    }
            //    return PatternID;
            //}
            //if (failedTraining)
            //{
            //    if (LvL5SuppTaskList.Count > 0)
            //    {
            //        int[] PatternID = new int[difficultyLevel];
            //        string[] PatternIDString = new string[difficultyLevel];

            //        PatternIDString = LvL5SuppTaskList[0].Split(fieldSeperator);

            //        LvL5SuppTaskList.RemoveAt(0);

            //        for (int i = 0; i < difficultyLevel; i++)
            //        {
            //            PatternID[i] = int.Parse(PatternIDString[i]);
            //        }
            //        return PatternID;
            //    }
            //}
            //else {
                if (layout == Layout.Flat || layout == Layout.LimitedFlat)
                {
                    if (LvL5FlatTaskList.Count > 0)
                    {
                        int[] PatternID = new int[difficultyLevel];
                        string[] PatternIDString = new string[difficultyLevel];

                        PatternIDString = LvL5FlatTaskList[0].Split(fieldSeperator);

                        LvL5FlatTaskList.RemoveAt(0);

                        for (int i = 0; i < difficultyLevel; i++)
                        {
                            PatternID[i] = int.Parse(PatternIDString[i]);
                        }
                        return PatternID;
                    }
                }
                else if (layout == Layout.FullCircle)
                {
                    if (LvL5CircularTaskList.Count > 0)
                    {
                        int[] PatternID = new int[difficultyLevel];
                        string[] PatternIDString = new string[difficultyLevel];

                        PatternIDString = LvL5CircularTaskList[0].Split(fieldSeperator);

                        LvL5CircularTaskList.RemoveAt(0);

                        for (int i = 0; i < difficultyLevel; i++)
                        {
                            PatternID[i] = int.Parse(PatternIDString[i]);
                        }
                        return PatternID;
                    }
                }
            //} 
        }
        return null;
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

        currentPattern = GetCurrentPattern();

        if (currentPattern != null)
        {
            for (int i = 0; i < currentPattern.Length; i++)
            {
                cards[currentPattern[i]].GetComponent<Card>().filled = true;
            }
        }
        else
            Debug.LogError("Pattern Used Up!!");

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
                    localCards[index].transform.localEulerAngles = new Vector3(0, 0, 0);
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

        CardGame.localPosition = new Vector3(0, adjustedHeight, 0);

        switch (localLayout) {
            case Layout.Flat:
                transform.localPosition = new Vector3(0, adjustedHeight, -1);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                //FilterCube.gameObject.SetActive(false);
                //EdgeIndicator.gameObject.SetActive(false);
                break;
            case Layout.LimitedFlat:
                transform.localPosition = new Vector3(0, adjustedHeight, -1);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                //FilterCube.gameObject.SetActive(true);
                //EdgeIndicator.gameObject.SetActive(false);
                break;
            case Layout.FullCircle:
                transform.localPosition = new Vector3(0, adjustedHeight, 0);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                //FilterCube.gameObject.SetActive(false);
                //EdgeIndicator.gameObject.SetActive(true);
                break;
            case Layout.LimitedFullCircle:
                transform.localPosition = new Vector3(0, adjustedHeight, 0);
                GameObject.Find("PreferableStand").transform.localPosition = new Vector3(0, 0.01f, 0);
                //FilterCube.gameObject.SetActive(true);
                //EdgeIndicator.gameObject.SetActive(true);
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
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
        else
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
    }

    // timer function
    private void TimerAndCheckScan() {
        // timer function
        if (LocalMemoryTime >= 0 && startCount)
            LocalMemoryTime -= Time.deltaTime;

        if (LocalMemoryTime < 3f) {
            if (!soundPlayed)
            {
                AudioSource.PlayClipAtPoint(TimesUp, transform.position);
                soundPlayed = true;
            }
        }

        CheckFilledScanned();
        CheckEverythingSelected();

        if (LocalMemoryTime < 0.05f)
        {
            if (allSeen && allSelected)
                HidePattern(false);
            //else {

            //}
            //else
            //{
            //    correctTrial = false;
            //    PrepareExperiment();
            //}
        }
        /// (OLD) click touchpad to finish acquisition
        //else
        //{
        //    // assign left and right controllers interaction use
        //    if (mainHandCE != null)
        //    {
        //        if (mainHandCE.touchpadPressed)
        //        {
        //            localTouchpadPressed = true;
        //        }
        //        else
        //        {
        //            if (allSeen && allSelected)
        //            {
        //                if (localTouchpadPressed)
        //                    HidePattern(false);
        //            }

        //            localTouchpadPressed = false;
        //        }
        //    }
        //} 
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
                        if (go.transform.GetChild(0).GetComponent<Renderer>().isVisible) {
                            go.GetComponent<Card>().seen = true;
                            if (!go.GetComponent<Card>().seenLogged) {
                                WriteInteractionToLog(go.name + " seen");
                                seenTimeLog.Add(scanTime);
                                go.GetComponent<Card>().seenLogged = true;
                            }
                        }
                    }
                }
                else
                { // check in boxed and in view port in limited flat layout
                    if (Vector3.Distance(Camera.main.transform.position, go.transform.position) < 1.4f) {
                        Vector3 wtvp = Camera.main.WorldToViewportPoint(go.transform.position);

                        if (wtvp.x < 0.7f && wtvp.x > 0.3f && wtvp.y < 0.8f && wtvp.y > 0.2f && wtvp.z > 0f)
                        {
                            if (go.transform.GetChild(0).GetComponent<Renderer>().isVisible)
                            {
                                go.GetComponent<Card>().seen = true;
                                if (!go.GetComponent<Card>().seenLogged)
                                {
                                    WriteInteractionToLog(go.name + " seen");
                                    seenTimeLog.Add(scanTime);
                                    go.GetComponent<Card>().seenLogged = true;
                                }
                            }
                        }
                    }
                }

                if (!go.GetComponent<Card>().seen)
                    allSeen = false;
            }
        }
    }

    // check user selected all filled cards
    private void CheckEverythingSelected()
    {
        if (selectTime >= 0 && startCount && !allSelected)
            selectTime += Time.deltaTime;

        allSelected = true;

        // assign left and right controllers interaction touch
        if (mainHandIT == null)
            mainHandIT = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractTouch>();

        // assign main hand index
        if (mainHandIndex == -1)
            mainHandIndex = (int)GameObject.Find("Controller (right)").GetComponent<VRTK_TrackedController>().index;

        if (mainHandIT.GetTouchedObject() != null)
        {
            // haptic function
            SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);

            GameObject selectedCard = mainHandIT.GetTouchedObject();
            selectedCard.GetComponent<Card>().selected = true;
            if (!selectedCard.GetComponent<Card>().selectLogged)
            {
                WriteInteractionToLog(selectedCard.name + " selected");
                selectTimeLog.Add(selectTime);
                selectedCard.GetComponent<Card>().selectLogged = true;
            }
        }

        /// (OLD) click to select
        // assign left and right controllers interaction use
        //if (mainHandIU == null)
        //{
        //    if (mainHand == 0 && GameObject.Find("LeftControllerAlias") != null)
        //        mainHandIU = GameObject.Find("LeftControllerAlias").GetComponent<VRTK_InteractUse>();
        //    else if (mainHand == 1 && GameObject.Find("RightControllerAlias") != null)
        //        mainHandIU = GameObject.Find("RightControllerAlias").GetComponent<VRTK_InteractUse>();
        //}


        //if (mainHandIU.GetUsingObject() != null)
        //{
        //    GameObject selectedCard = mainHandIU.GetUsingObject();
        //    selectedCard.GetComponent<Card>().selected = true;
        //    if (!selectedCard.GetComponent<Card>().selectLogged)
        //    {
        //        selectTimeLog.Add(selectTime);
        //        selectedCard.GetComponent<Card>().selectLogged = true;
        //    }
        //}

        foreach (GameObject go in cards)
        {
            if (IsCardFilled(go)) {
                if (!go.GetComponent<Card>().selected) {
                    allSelected = false;
                }
            }
        }
    }


    private void ReadPatternsFromInput() {
        // pattern 2
        string[] lines = new string[10];

        lines = Patterns2.text.Split(lineSeperater);

        LvL2TaskList.AddRange(lines);

        // shuffle order for lists
        for (int i = 0; i < LvL2TaskList.Count; i++)
        {
            string temp = LvL2TaskList[i];
            int randomIndex = Random.Range(i, LvL2TaskList.Count);
            LvL2TaskList[i] = LvL2TaskList[randomIndex];
            LvL2TaskList[randomIndex] = temp;
        }

        // pattern 3
        lines = new string[40];

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

        // pattern 5
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

        // fixed pattern 5 flat
        lines = new string[20];
        lines = Patterns5Flat.text.Split(lineSeperater);
        LvL5FlatTaskList.AddRange(lines);

        // fixed pattern 5 flat
        lines = new string[20];
        lines = Patterns5Circular.text.Split(lineSeperater);
        LvL5CircularTaskList.AddRange(lines);

        // supplementary pattern 5 flat
        lines = new string[20];
        lines = Patterns5Supp.text.Split(lineSeperater);
        LvL5SuppTaskList.AddRange(lines);

        // game task
        lines = new string[40];
        lines = GameTask.text.Split(lineSeperater);
        GameTaskList.AddRange(lines);
    }

    /// Log related functions START
    // write to log file
    private void WritingToLog()
    {
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
            writer.WriteLine(GetFixedTime() + "," + StartSceneScript.adjustedHeight + "," + GetTrialNumber() + "," + GetTrialID() + "," + StartSceneScript.ParticipantID + "," + StartSceneScript.ExperimentSequence + "," +
                GetLayout() + "," + GetDifficulty() + "," + GetGameState() + "," + VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(mainLogController.position) + "," + VectorToString(mainLogController.eulerAngles) + "," + GetPadPressed() + "," + GetTriggerPressed());
            writer.Flush();
        }

        if (writerHead != null && Camera.main != null && mainLogController != null)
        {
            writerHead.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + StartSceneScript.ParticipantID + "," + StartSceneScript.ExperimentSequence + "," +
                GetLayout() + "," + GetDifficulty() + "," + GetGameState() + "," + VectorToString(Camera.main.transform.position) + "," + VectorToString(Camera.main.transform.eulerAngles) + "," +
                VectorToString(mainLogController.position) + "," + VectorToString(mainLogController.eulerAngles));
            writerHead.Flush();
        }
    }

    private void WriteAnswerToLog() {
        if (writerAnswer != null)
        {
            writerAnswer.WriteLine(StartSceneScript.ParticipantID + "," + GetTrialNumber() + "," + GetTrialID() + "," + GetLayout() + "," +
                GetDifficulty() + "," + GetAccuracy() + "," + GetSeenTime() + "," + GetSelectTime());
            writerAnswer.Flush();
        }
    }

    public void WriteInteractionToLog(string info) {
        if (writerInteraction != null)
        {
            if (info.Contains("seen"))
                writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + 
                    StartSceneScript.ParticipantID + "," + GetLayout() + "," + "Card," + info.Split(' ')[0].Remove(0, 4) + ",,,");
            else if(info.Contains("selected"))
                writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," +
                   StartSceneScript.ParticipantID + "," + GetLayout() + "," + "Card,," + info.Split(' ')[0].Remove(0, 4) + ",,");
            else if(info.Contains("answered"))
                writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," +
                   StartSceneScript.ParticipantID + "," + GetLayout() + "," + "Card,,," + info.Split(' ')[0].Remove(0, 4) + ",");
            else if (info.Contains("played"))
                writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," +
                   StartSceneScript.ParticipantID + "," + GetLayout() + "," + "CardGame,,,," + info.Split(' ')[0]);
            else
                writerInteraction.WriteLine(GetFixedTime() + "," + GetTrialNumber() + "," + GetTrialID() + "," + StartSceneScript.ParticipantID + "," + GetLayout() + "," + info + ",,,");
            writerInteraction.Flush();
        }
    }

    private void WriteCardsLog() {
        if (writerTrialCards != null) {
            string final = "";

            foreach (GameObject card in selectedCards)
            {
                final += card.name.Split(' ')[0].Remove(0, 4) + ",";
            }

            final.Remove(final.Length - 1);

            writerTrialCards.WriteLine(final);
            writerTrialCards.Flush();
        }

        if (writerAnswerCards != null) {
            string final = "";

            foreach (int cardIndex in currentPattern)
            {
                int cardtmp = cardIndex;
                final += cardtmp + ",";
            }

            final.Remove(final.Length - 1);

            writerAnswerCards.WriteLine(final);
            writerAnswerCards.Flush();
        }
    }


    float GetFixedTime()
    {
        float finalTime = 0;
        if (StartSceneScript.lastTimePast != 0)
            finalTime = StartSceneScript.lastTimePast + Time.fixedTime;
        else
            finalTime = Time.fixedTime;
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
        //int tmp = (trialNo - 1) % 7;
        //if (tmp == 0)
        //    return "Training";
        //else
        //    return (trialNo - (int)((trialNo - 1) / 7) - 1).ToString();

        if (trialNo == 1 || trialNo == 2)
            return "Training";
        else
            return (trialNo - 2) + "";
            //return (trialNo - (int)((trialNo - 1) / 10) * 2 - 2).ToString();
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
            case GameState.Distractor:
                return "distractor";
            default:
                return "";
        }
    }

    // Get current cards layouts based on sequence
    private string GetLayout()
    {
        switch (layout) {
            case Layout.Flat:
                return "Flat";
            case Layout.FullCircle:
                return "Full Circle";
            case Layout.LimitedFlat:
                return "Limited Flat";
            case Layout.LimitedFullCircle:
                return "Limited Full Circle";
            default:
                return "NULL";
        }
    }

    private string GetDifficulty()
    {
        return difficultyLevel + "";
    }

    private string GetAccuracy() {
        return accurateNumber + "";
    }

    //private string GetShootAccuracy()
    //{
    //    return Basket.shootCount + "";
    //}

    private string GetSeenTime()
    {
        if (difficultyLevel == 2)
            return seenTimeLog[0] + "," + seenTimeLog[1] + "," + "," + ",";
        else if (difficultyLevel == 3)
            return seenTimeLog[0] + "," + seenTimeLog[1] + "," + seenTimeLog[2] + "," + ",";
        else if (difficultyLevel == 5)
            return seenTimeLog[0] + "," + seenTimeLog[1] + "," + seenTimeLog[2] + "," + seenTimeLog[3] + "," + seenTimeLog[4];
        return "";
    }

    private string GetSelectTime()
    {
        if (difficultyLevel == 2)
            return selectTimeLog[0] + "," + selectTimeLog[1] + "," + "," + ",";
        else if (difficultyLevel == 3)
            return selectTimeLog[0] + "," + selectTimeLog[1] + "," + selectTimeLog[2] + "," + ",";
        else if (difficultyLevel == 5)
            return selectTimeLog[0] + "," + selectTimeLog[1] + "," + selectTimeLog[2] + "," + selectTimeLog[3] + "," + selectTimeLog[4];
        return "";
    }

    private bool GetPadPressed()
    {
        if (mainHandCE == null)
        {
            mainHandCE = GameObject.Find("RightControllerAlias").GetComponent<VRTK_ControllerEvents>();
        }

        if (mainHandCE.touchpadPressed)
            return true;
        else
            return false;
    }

    private bool GetTriggerPressed()
    {
        if (mainHandCE == null)
        {
            mainHandCE = GameObject.Find("RightControllerAlias").GetComponent<VRTK_ControllerEvents>();
        }

        if (mainHandCE.triggerClicked)
            return true;
        else
            return false;
    }

    string VectorToString(Vector3 v)
    {
        string text;
        text = v.x + "," + v.y + "," + v.z;
        return text;
    }

    /// Log functions END

    /// Card property related functions START
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
    /// Card property related functions END

    /// general functions
    private void PrintTextToScreen(Text textBoard, string text)
    {
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

    public int RandomNumber(int min, int max)
    {
        return Random.Range(min, max);
    }
}
