using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.GrabAttachMechanics;

public class Basket : MonoBehaviour
{
    //public AudioClip Applaud;
    public Transform Ball;
    public Transform Stand;
    public bool Distractor = false;

    private bool played = false;
    private bool scored = false;
    private bool overHoop = false;
    public static int shootCount = 0;
    private float headToChest = 0.8f;
    private float delta = 0.75f;
    private float ballDelta = 0.144f;

    private ExperimentManager em;

    private void Start()
    {
        if (GameObject.Find("ExperimentManager") != null)
        {
            em = GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>();
        }

        Stand.position = new Vector3(0, (StartSceneScript.adjustedHeight + delta - headToChest) / 2, 0.5f);
        Stand.localScale = new Vector3(0.3f, StartSceneScript.adjustedHeight + delta - headToChest, 0.3f);
        Ball.position = new Vector3(0, StartSceneScript.adjustedHeight + delta - headToChest + ballDelta, 0.5f);
        transform.position = new Vector3(-0.5f, StartSceneScript.adjustedHeight + 1 + delta, 1.29f);
    }

    private void Update()
    {
        if (Ball.GetComponent<VRTK_FixedJointGrabAttach>() != null)
        {
            Ball.GetComponent<VRTK_FixedJointGrabAttach>().precisionGrab = true;
        }

        if (!played)
        {
            if (Ball.position.y > StartSceneScript.adjustedHeight + 1 + delta + 0.2f)
                overHoop = true;

            if (Ball.position.y > StartSceneScript.adjustedHeight + delta - headToChest + 0.15f)
                Stand.gameObject.SetActive(false);

            if (Ball.position.y < 0.2f)
                played = true;
        }
        else
            ResetBallPosition();
    }

    private void ResetBallPosition() {
        Stand.gameObject.SetActive(true);

        // reset ball position
        if (Ball.gameObject.activeSelf)
        {
            Ball.position = new Vector3(0, StartSceneScript.adjustedHeight + delta - headToChest + ballDelta, 0.5f);
            Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            played = false;
            overHoop = false;

            if (scored)
                scored = false;
        }
    }

    void OnTriggerEnter()
    {
        if (!scored && overHoop) {
            scored = true;
            shootCount++;

            if (em != null) {
                em.WriteInteractionToLog("Bingo");
                em.shootTotalNumber++;
            }
            //AudioSource.PlayClipAtPoint(Applaud, transform.position);
        }
    }

    public void ResetScore() {
        shootCount = 0;
    }

}
