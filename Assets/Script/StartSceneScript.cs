using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.UI;
using VRTK;
using TMPro;
using Random = UnityEngine.Random;

public class StartSceneScript : MonoBehaviour
{
    public static int controllerHand = 1;
    public static float adjustedHeight = 1;
    public static int ExperimentSequence;
    public static int ParticipantID;
    public static string CurrentDateTime;
    public static int PublicTrialNumber;
    public static float lastTimePast;
    public static int distratorType = 1;

    [Header("Do Not Change")]
    public int trainingCount = 0; // 0: start 8: GO TO Experiment

    public Text instruction;
    public Text instruction2;
    public Transform FootPrint;
    public TextMeshProUGUI trackPadText;

    //public Transform Hoop;
    //public Transform Stand;
    //public Transform Ball;
    public Transform CardGame;
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
    private List<GameObject> selectedGameCards;
    private VRTK_InteractTouch mainHandIT;
    private int mainHandIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        if (rightCE == null) {
            rightCE = GameObject.Find("RightControllerAlias").GetComponent<VRTK_ControllerEvents>();
        }

        selectedCards = new List<GameObject>();
        selectedGameCards = new List<GameObject>();
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

            switch (ExperimentID % 2)
            {
                case 1:
                    ExperimentSequence = 1;
                    break;
                case 0:
                    ExperimentSequence = 2;
                    break;
                default:
                    break;
            }

            // old conditions
            //switch (ExperimentID % 4)
            //{
            //    case 1:
            //        ExperimentSequence = 1;
            //        break;
            //    case 2:
            //        ExperimentSequence = 2;
            //        break;
            //    case 3:
            //        ExperimentSequence = 3;
            //        break;
            //    case 0:
            //        ExperimentSequence = 4;
            //        break;
            //    default:
            //        break;
            //}


        }
        else
        { // testing stream
            ExperimentSequence = 1;
        }

        for (int i = 0; i < CardGame.childCount; i++)
        {
            Vector3 temp = CardGame.GetChild(i).localPosition;
            int randomIndex = Random.Range(i, CardGame.childCount);
            CardGame.GetChild(i).localPosition = CardGame.GetChild(randomIndex).localPosition;
            CardGame.GetChild(randomIndex).localPosition = temp;
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

            // interaction log
            string writerInteractionFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_Interaction.csv";
            writer = new StreamWriter(writerInteractionFilePath, false);
            writer.WriteLine("TimeSinceStart,TrialNo,TrialID,ParticipantID,Layout,Info,CardSeen,CardSelected,CardAnswered,CardPlayed");
            writer.Close();

            // Answers data log
            string writerAnswerFilePath = "Assets/ExperimentData/ExperimentLog/Participant " + ParticipantID + "/Participant_" + ParticipantID + "_Answers.csv";
            writer = new StreamWriter(writerAnswerFilePath, false);
            writer.WriteLine("ParticipantID,TrialNo,TrialID,Layout,Difficulty,AnswerAccuracy,Card1SeenTime,Card2SeenTime,Card3SeenTime,Card4SeenTime,Card5SeenTime," +
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
        if (Input.GetKeyDown("b"))
            trainingCount--;

        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            if (TrialNumber == 0)
            {
                if (rightCE != null)
                {
                    switch (trainingCount)
                    {
                        case 0:
                            instruction.text = "Explore the space.";
                            instruction2.text = "Explore the space.";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 2;
                                touchPadPressed = false;
                            }
                            break;
                        //case 1:
                        //    Cards.gameObject.SetActive(false);
                        //    instruction.text = "Three phases in the experiment.";
                        //    instruction2.text = "Three phases in the experiment.";

                        //    if (rightCE.touchpadPressed)
                        //        touchPadPressed = true;
                        //    if (!rightCE.touchpadPressed && touchPadPressed)
                        //    {
                        //        trainingCount = 2;
                        //        touchPadPressed = false;
                        //    }
                        //    break;
                        case 2:
                            Cards.gameObject.SetActive(true);
                            if (!showPatternFlag)
                            {
                                Cards.position = new Vector3(0, Camera.main.transform.position.y, 0);
                                adjustedHeight = Camera.main.transform.position.y - 0.75f;

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

                            instruction.text = "Acquisition phase";
                            instruction2.text = "Acquisition phase";
                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                //trainingCount = 3;
                                trainingCount = 4;
                                touchPadPressed = false;
                            }
                            break;
                        //case 3:
                        //    FilterCube.gameObject.SetActive(true);

                        //    if (rightCE.touchpadPressed)
                        //        touchPadPressed = true;
                        //    if (!rightCE.touchpadPressed && touchPadPressed)
                        //    {
                        //        trainingCount = 4;
                        //        touchPadPressed = false;
                        //    }
                        //    break;
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
                            //FilterCube.gameObject.SetActive(false);

                            if (distratorType == 0)
                            {
                                instruction.text = "3 2 8 5 9";
                                instruction2.text = "3 2 8 5 9";

                                //if (!Hoop.gameObject.activeSelf)
                                //    Hoop.gameObject.SetActive(true);

                                //if (!Stand.gameObject.activeSelf)
                                //    Stand.gameObject.SetActive(true);

                                //if (!Ball.gameObject.activeSelf)
                                //    Ball.gameObject.SetActive(true);

                                if (rightCE.touchpadPressed)
                                    touchPadPressed = true;
                                if (!rightCE.touchpadPressed && touchPadPressed)
                                {
                                    trainingCount = 9;
                                    touchPadPressed = false;
                                }
                            }
                            else {
                                instruction.text = "3";
                                instruction2.text = "3";

                                instruction.transform.parent.parent.position = new Vector3(0, CardGame.position.y + 0.6f, 1f);

                                CardGame.gameObject.SetActive(true);

                                GameObject selectedCard2 = null;
                                if (mainHandIT.GetTouchedObject() != null)
                                {
                                    // haptic function
                                    SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);
                                    if (selectedGameCards.Count < 1)
                                    {
                                        selectedCard2 = mainHandIT.GetTouchedObject();
                                        if (!selectedGameCards.Contains(selectedCard2))
                                            selectedGameCards.Add(selectedCard2);

                                        selectedCard2.GetComponent<Card>().filled = true;
                                        selectedCard2.GetComponent<Card>().seen = true;
                                        selectedCard2.GetComponent<Card>().selected = true;
                                    }
                                }

                                if (rightCE.touchpadPressed)
                                    touchPadPressed = true;
                                if (!rightCE.touchpadPressed && touchPadPressed)
                                {
                                    trainingCount = 5;
                                    touchPadPressed = false;
                                }
                            }
                            
                            break;
                        case 9:
                            instruction.text = "";
                            instruction2.text = "";

                            CardGame.gameObject.SetActive(true);

                            GameObject selectedCard = null;
                            if (mainHandIT.GetTouchedObject() != null)
                            {
                                // haptic function
                                SteamVR_Controller.Input(mainHandIndex).TriggerHapticPulse(1500);
                                if (selectedGameCards.Count < 5) {
                                    selectedCard = mainHandIT.GetTouchedObject();
                                    if (!selectedGameCards.Contains(selectedCard))
                                        selectedGameCards.Add(selectedCard);

                                    selectedCard.GetComponent<Card>().filled = true;
                                    selectedCard.GetComponent<Card>().seen = true;
                                    selectedCard.GetComponent<Card>().selected = true;
                                }
                            }

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 5;
                                touchPadPressed = false;
                            }
                            break;
                        case 5:

                            instruction.transform.parent.parent.position = new Vector3(0, 1.8f, 1.35f);

                            CardGame.gameObject.SetActive(false);
                            Cards.gameObject.SetActive(true);
                            FootPrint.gameObject.SetActive(false);

                            //if (Hoop.gameObject.activeSelf)
                            //    Hoop.gameObject.SetActive(false);

                            //if (Stand.gameObject.activeSelf)
                            //    Stand.gameObject.SetActive(false);

                            //if (Ball.gameObject.activeSelf)
                            //    Ball.gameObject.SetActive(false);

                            selectedCard = null;
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

                            instruction.text = "Retrieval phase";
                            instruction2.text = "Retrieval phase";

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 6;
                                touchPadPressed = false;
                            }
                            break;
                        case 6:
                            instruction.text = "Note: when you see the image below, go back to the original position.";
                            instruction2.text = "Note: when you see the image below, go back to the original position.";

                            FootPrint.gameObject.SetActive(true);
                            Cards.gameObject.SetActive(false);

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 7;
                                FootPrint.gameObject.SetActive(false);
                                touchPadPressed = false;
                            }
                            break;
                        case 7:
                            trackPadText.text = "Start";
                            instruction.text = "Now, please stand still and press the <color=green>Start</color> button to start the experiment.";
                            instruction2.text = "Now, please stand still and press the <color=green>Start</color> button to start the experiment.";

                            if (rightCE.touchpadPressed)
                                touchPadPressed = true;
                            if (!rightCE.touchpadPressed && touchPadPressed)
                            {
                                trainingCount = 8;
                                touchPadPressed = false;
                            }
                            break;
                        case 8:
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
