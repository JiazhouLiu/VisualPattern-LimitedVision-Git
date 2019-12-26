using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.UI;
using VRTK;
using TMPro;

public class StartSceneScript : MonoBehaviour
{
    public static int controllerHand = 1;
    public static float adjustedHeight = 1;
    public static int ExperimentSequence;
    public static int ParticipantID;
    public static string CurrentDateTime;
    public static int PublicTrialNumber;
    public static float lastTimePast;

    [Header("Do Not Change")]
    public int trainingCount = 0; // 0: Space, 1: Card, 2: Basketball 3: GO TO Experiment

    public Text instruction;
    public TextMeshProUGUI trackPadText;

    public Transform Hoop;
    public Transform Ball;
    public Transform Cards;
    public Transform FilterCube;

    [Header("Experiment Parameter")]
    public int ExperimentID;
    public int TrialNumber;

    private VRTK_ControllerEvents rightCE;

    
    private bool touchPadPressed = false;
    private bool showPatternFlag = false;
    private List<GameObject> cardLists;
    private List<GameObject> selectedCards;
    private VRTK_InteractTouch mainHandIT;
    private int mainHandIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (rightCE == null) {
            rightCE = GameObject.Find("RightControllerAlias").GetComponent<VRTK_ControllerEvents>();
        }

        selectedCards = new List<GameObject>();
        cardLists = new List<GameObject>
        {
            Cards.GetChild(0).gameObject,
            Cards.GetChild(1).gameObject,
            Cards.GetChild(2).gameObject,
            Cards.GetChild(3).gameObject
        };

        PublicTrialNumber = TrialNumber;

        if (ExperimentID > 0)
        {
            ParticipantID = ExperimentID;

            switch (ExperimentID % 4)
            {
                case 1:
                    ExperimentSequence = 1;
                    break;
                case 2:
                    ExperimentSequence = 2;
                    break;
                case 3:
                    ExperimentSequence = 3;
                    break;
                case 0:
                    ExperimentSequence = 4;
                    break;
                default:
                    break;
            }
        }
        else
        { // testing stream
            ExperimentSequence = 1;
        }

        if (TrialNumber == 0)
        {
            CurrentDateTime = GetDateTimeString();

            // Raw data log
            string writerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_RawData.csv";
            StreamWriter writer = new StreamWriter(writerFilePath, false);
            string logFileHeader = "TimeSinceStart,UserHeight,TrialNo,TrialID,ParticipantID,ExperimentSequence,Layout,Difficulty,TrialState,CameraPosition.x," +
                "CameraPosition.y,CameraPosition.z,CameraEulerAngles.x,CameraEulerAngles.y,CameraEulerAngles.z,MainControllerPosition.x,MainControllerPosition.y," +
                "MainControllerPosition.z,MainControllerEulerAngles.x,MainControllerEulerAngles.y,MainControllerEulerAngles.z,MainPadPressed,MainTriggerPressed";
            writer.WriteLine(logFileHeader);
            writer.Close();

            // head and hand data log
            string writerHeadFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_HeadAndHand.csv";
            writer = new StreamWriter(writerHeadFilePath, false);
            writer.WriteLine("TimeSinceStart,TrialNo,TrialID,ParticipantID,ExperimentSequence,Layout,Difficulty,TrialState,CameraPosition.x," +
                "CameraPosition.y,CameraPosition.z,CameraEulerAngles.x,CameraEulerAngles.y,CameraEulerAngles.z,MainControllerPosition.x,MainControllerPosition.y," +
                "MainControllerPosition.z,MainControllerEulerAngles.x,MainControllerEulerAngles.y,MainControllerEulerAngles.z");
            writer.Close();

            // Answers data log
            string writerAnswerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_Answers.csv";
            writer = new StreamWriter(writerAnswerFilePath, false);
            writer.WriteLine("ParticipantID,TrialNo,TrialID,Layout,Difficulty,AnswerAccuracy,ShootAccuracy,Card1SeenTime,Card2SeenTime,Card3SeenTime,Card4SeenTime,Card5SeenTime," +
                "Card1SelectTime,Card2SelectTime,Card3SelectTime,Card4SelectTime,Card5SelectTime");
            writer.Close();
        }
        else
        {
            string lastFileName = "";

            string folderPath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/";
            DirectoryInfo info = new DirectoryInfo(folderPath);
            FileInfo[] fileInfo = info.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains("Participant_" + ParticipantID + "_RawData.csv") && !file.Name.Contains("meta"))
                {
                    lastFileName = file.Name;
                }
            }
            if (lastFileName == "")
            {
                Debug.LogError("No previous file found!");
            }
            else
            {
                string writerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/" + lastFileName;
                string lastLine = File.ReadAllLines(writerFilePath)[File.ReadAllLines(writerFilePath).Length - 1];
                float lastTime = float.Parse(lastLine.Split(',')[0]);
                float height = float.Parse(lastLine.Split(',')[1]);

                lastTimePast = lastTime;
                adjustedHeight = height;
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            if (TrialNumber == 0)
            {
                if (rightCE != null)
                {
                    switch (trainingCount)
                    {
                        case 0:
                            instruction.text = "The place inside walls is safe to move around.\n\n" +
                                "Now, please move to the edges of the room and return to the original point. " +
                                "Press <color=green>Next</color> button to see the next instruction.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 1;
                                touchPadPressed = false;
                            }
                            break;
                        case 1:
                            instruction.text = "The main task is to remember the patterns (white cards) on 3 * 12 card matrix with different layouts. " +
                                "There are three phases in the experiment: acquisition, play and retrieval.\n\n " +
                                "Press <color=green>Next</color> button to see the next instruction.";

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 2;
                                touchPadPressed = false;
                            }
                            break;
                        case 2:
                            Cards.gameObject.SetActive(true);
                            if (!showPatternFlag)
                            {
                                Cards.position = new Vector3(0, Camera.main.transform.position.y, 0);

                                // flip to the front
                                foreach (GameObject card in cardLists)
                                {
                                    if (IsCardFilled(card))
                                        SetCardsColor(card.transform, Color.white);
                                }
                                showPatternFlag = true;
                            }

                            CheckFilledScanned();
                            CheckEverythingSelected();

                            instruction.text = "During the acquisition phases, " +
                                "you need to see all the white cards (border turns yellow) and use the controller to touch them (border turns blue). " +
                                "Green borders means you've done both. " +
                                "In order to proceed, you have to let all whites have green borders in 15 seconds. \n\n" +
                                "Now, please touch the cards around you. " +
                                "Press <color=green>Next</color> button to see the next instruction.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 3;
                                touchPadPressed = false;
                            }
                            break;
                        case 3:
                            FilterCube.gameObject.SetActive(true);

                            instruction.text = "In some cases, you'll have a black curtain around you to limit your vision. \n\n" +
                                "Press <color=green>Next</color> button to see the next instruction.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 4;
                                touchPadPressed = false;
                            }
                            break;
                        case 4:
                            // reset card property

                            foreach (GameObject go in cardLists)
                            {
                                go.GetComponent<Card>().seen = false;
                                go.GetComponent<Card>().seenLogged = false;
                                go.GetComponent<Card>().selected = false;
                                go.GetComponent<Card>().selectLogged = false;
                            }
                            // flip to the back
                            foreach (GameObject card in cardLists)
                            {
                                if (IsCardFilled(card))
                                    SetCardsColor(card.transform, Color.black);
                                card.transform.localEulerAngles = Vector3.zero;
                            }
                            // enable the interactable feature
                            foreach (GameObject card in cardLists)
                            {
                                card.GetComponent<VRTK_InteractableObject>().enabled = true;
                            }

                            Cards.gameObject.SetActive(false);
                            FilterCube.gameObject.SetActive(false);

                            instruction.text = "During the play phase, you have 15 seconds to play the basketball game. " +
                                "Use <color=red>Trigger</color> button to grab the ball and throw it into the basket. " +
                                "We'll record your score at the end and a $50 prize will be given to the best player.\n\n" +
                                "Now, please get familiar with the game. Press <color=green>Next</color> button to see the next instruction.";

                            if (!Hoop.gameObject.activeSelf)
                                Hoop.gameObject.SetActive(true);

                            if (!Ball.gameObject.activeSelf)
                                Ball.gameObject.SetActive(true);

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 5;
                                touchPadPressed = false;
                            }
                            break;
                        case 5:
                            Cards.gameObject.SetActive(true);

                            if (Hoop.gameObject.activeSelf)
                                Hoop.gameObject.SetActive(false);

                            if (Ball.gameObject.activeSelf)
                                Ball.gameObject.SetActive(false);

                            GameObject selectedCard = null;
                            if (mainHandIT.GetTouchedObject() != null)
                            {
                                // haptic function
                                SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);

                                if (!IsCardRotating(mainHandIT.GetTouchedObject()))
                                {
                                    selectedCard = mainHandIT.GetTouchedObject();
                                    if (!IsCardFlipped(selectedCard) && selectedCards.Count < 2) // not flipped
                                    {
                                        selectedCards.Add(selectedCard);
                                        selectedCard.GetComponent<Card>().flipped = true;
                                        StartCoroutine(Rotate(selectedCard.transform, new Vector3(0, 180, 0), 0.5f));
                                        SetCardsColor(selectedCard.transform, Color.white);
                                    }
                                }
                            }

                            instruction.text = "During the retrieval phase, you need to select the white cards as the pattern. " +
                                "You cannot undo the selection. Your selection has a limit same as the white card number. " +
                                "You will be given the result of your selection at the end. \n\n" +
                                "Now, please touch the cards around you to select the cards. " +
                                "Press <color=green>Next</color> button to see the next instruction.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 6;
                                touchPadPressed = false;
                            }
                            break;

                        case 6:
                            trackPadText.text = "Start";
                            instruction.text = "Now, please stand still and press the <color=green>Start</color> button to start the experiment.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 7;
                                touchPadPressed = false;
                            }
                            break;
                        case 7:
                            if (Camera.main != null)
                                adjustedHeight = Camera.main.transform.position.y - 0.75f;
                            SceneManager.LoadScene("Experiment", LoadSceneMode.Single);
                            break;
                        default:
                            break;
                    }
                }
            }
            else {
                trackPadText.text = "Start";
                instruction.text = "Now, please stand still and press the <color=green>Start</color> button to start the experiment.";
                if (rightCE.touchpadPressed)
                    touchPadPressed = true;
                if (!rightCE.touchpadPressed && touchPadPressed)
                    SceneManager.LoadScene("Experiment", LoadSceneMode.Single);
            }
        }
    }

    string GetDateTimeString()
    {
        return DateTime.Now.Month.ToString("D2") + DateTime.Now.Day.ToString("D2") + "-" + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Minute.ToString("D2") + DateTime.Now.Second.ToString("D2");
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // check user viewport
    private void CheckFilledScanned()
    {
        foreach (GameObject go in cardLists)
        {
            if (IsCardFilled(go))
            {
                Vector3 wtvp = Camera.main.WorldToViewportPoint(go.transform.position);

                if (wtvp.x < 0.7f && wtvp.x > 0.3f && wtvp.y < 0.8f && wtvp.y > 0.2f && wtvp.z > 0f)
                {
                    if (go.transform.GetChild(0).GetComponent<Renderer>().isVisible)
                    {
                        go.GetComponent<Card>().seen = true;
                        if (!go.GetComponent<Card>().seenLogged)
                            go.GetComponent<Card>().seenLogged = true;
                    }
                }
            }
        }
    }

    // check user selected all filled cards
    private void CheckEverythingSelected()
    {
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
                selectedCard.GetComponent<Card>().selectLogged = true;
        }
    }

    // Check if card filled property is true
    private bool IsCardFilled(GameObject go)
    {
        if (go.GetComponent<Card>().filled)
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

    // Check if card flipped property is true
    private bool IsCardFlipped(GameObject go)
    {
        if (go.GetComponent<Card>().flipped)
            return true;
        return false;
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

    // Set Card Color
    private void SetCardsColor(Transform t, Color color)
    {
        if (color == Color.white)
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
        else
            t.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
    }
}
