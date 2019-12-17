using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerColor : MonoBehaviour {
    //public GameObject touchBarPrefab;
    public Material white;
    public Material red;
    //public Material orange;
    //public Material yellow;
    public Material green;
    //public Material sky;
    //public Material blue;
    //public Material purple;

    //GameObject touchbar;

    private void Start()
    {
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

        
        //if (transform.Find("tip") != null)
        //{
        //    if (transform.Find("tip").GetChild(0) != null)
        //    {
        //        if (transform.Find("tip").GetChild(0).childCount == 0)
        //        {
        //            touchbar = Instantiate(touchBarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //            touchbar.transform.SetParent(transform.Find("tip").GetChild(0));
        //            touchbar.transform.localPosition = new Vector3(0, -0.0172f, 0.0602f);
        //            touchbar.transform.localEulerAngles = new Vector3(90, 0, 0);

        //        }
        //    }
        //}

        //if (touchbar != null) {
        //    if (this.transform.parent.name == "Controller (left)")
        //    {
        //        touchbar.GetComponent<Renderer>().material.color = Color.blue;
        //    }
        //    else
        //    {
        //        touchbar.GetComponent<Renderer>().material.color = Color.green;
        //    }
        //}


        if (this.transform.childCount > 0)
        {
            GameObject body = this.transform.GetChild(0).gameObject;
            GameObject grip = this.transform.GetChild(1).gameObject;
            GameObject menu = this.transform.GetChild(2).gameObject;
            GameObject thumbstick = this.transform.GetChild(3).gameObject;
            GameObject trackpad = this.transform.GetChild(5).gameObject;
            GameObject trigger = this.transform.GetChild(6).gameObject;

            body.GetComponent<MeshRenderer>().material = white;
            grip.GetComponent<MeshRenderer>().material = white;
            thumbstick.GetComponent<MeshRenderer>().material = white;
            menu.GetComponent<MeshRenderer>().material = white;
            trigger.GetComponent<MeshRenderer>().material = red;
            trackpad.GetComponent<MeshRenderer>().material = green;
        }
	}
}
