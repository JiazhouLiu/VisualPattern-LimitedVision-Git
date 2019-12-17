using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ControllerType
{ 
    HTCVIVE,
    Odyssey
}

public class ControllerColor : MonoBehaviour {
    public Material white;
    public Material red;
    public Material green;

    public ControllerType controllerType;

    private int bodyIndex;
    private int gripIndex;
    private int grip2Index;
    private int menuIndex;
    private int thumbstickIndex;
    private int trackpadIndex;
    private int triggerIndex;
    private int systemIndex;

    private void Start()
    {

        if (controllerType == ControllerType.HTCVIVE)
        {
            bodyIndex = 1;
            gripIndex = 7;
            grip2Index = 8;
            menuIndex = 2;
            thumbstickIndex = -1;
            trackpadIndex = 13;
            triggerIndex = 16;
            systemIndex = 11;


            transform.parent.Find("TrackPadText").position = new Vector3(0, 0.01f, -0.05f);
            transform.parent.Find("TrackPadText").localEulerAngles = new Vector3(90, 0, 0);
            transform.parent.Find("TrackPadText").GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 12;
        }
        else if (controllerType == ControllerType.Odyssey) {
            bodyIndex = 0;
            gripIndex = 1;
            grip2Index = -1;
            menuIndex = 2;
            thumbstickIndex = 3;
            trackpadIndex = 5;
            triggerIndex = 6;
            systemIndex = -1;
        }


        if (GameObject.Find("ExperimentManager") != null)
        {
            if (GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>().mainHand == 0)
            {
                if (transform.parent.name == "Controller (right)")
                {
                    transform.parent.gameObject.SetActive(false);
                }
            }
            else if (GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>().mainHand == 1)
            {
                if (transform.parent.name == "Controller (left)")
                {
                    transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        if (transform.childCount > 0)
        {
            if (bodyIndex > 0)
            {
                GameObject body = transform.GetChild(bodyIndex).gameObject;
                body.GetComponent<MeshRenderer>().material = white;
            }

            if (gripIndex > 0)
            {
                GameObject grip = transform.GetChild(gripIndex).gameObject;
                grip.GetComponent<MeshRenderer>().material = white;
            }

            if (grip2Index > 0)
            {
                GameObject grip2 = transform.GetChild(grip2Index).gameObject;
                grip2.GetComponent<MeshRenderer>().material = white;
            }

            if (menuIndex > 0)
            {
                GameObject menu = transform.GetChild(menuIndex).gameObject;
                menu.GetComponent<MeshRenderer>().material = white;
            }

            if (thumbstickIndex > 0)
            {
                GameObject thumbstick = transform.GetChild(thumbstickIndex).gameObject;
                thumbstick.GetComponent<MeshRenderer>().material = white;
            }

            if (trackpadIndex > 0)
            {
                GameObject trackpad = transform.GetChild(trackpadIndex).gameObject;
                trackpad.GetComponent<MeshRenderer>().material = green;
            }

            if (triggerIndex > 0) {
                GameObject trigger = transform.GetChild(triggerIndex).gameObject;
                trigger.GetComponent<MeshRenderer>().material = red;
            }

            if (systemIndex > 0)
            {
                GameObject systemButton = transform.GetChild(systemIndex).gameObject;
                systemButton.GetComponent<MeshRenderer>().material = white;
            }
            
        }
	}
}
