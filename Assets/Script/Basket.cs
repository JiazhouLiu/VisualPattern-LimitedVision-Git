using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.GrabAttachMechanics;

public class Basket : MonoBehaviour
{
    public AudioClip Applaud;
    public AudioClip Tease;
    public Transform Ball;
    public Transform Stand;
    public bool Distractor = false;

    private bool played = false;
    private bool scored = false;
    private bool overHoop = false;
    public static int shootCount = 0;

    private void Update()
    {
        if (Ball.GetComponent<VRTK_FixedJointGrabAttach>() != null)
        {
            Ball.GetComponent<VRTK_FixedJointGrabAttach>().precisionGrab = true;
        }

        if (!played)
        {
            if (Ball.position.y > 3f)
                overHoop = true;

            if (Ball.position.y > 1.2f)
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
            Ball.transform.position = new Vector3(0, 1.144f, 0.5f);
            Ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            played = false;
            overHoop = false;

            if (!scored)
                AudioSource.PlayClipAtPoint(Tease, transform.position);
            else
                scored = false;
        }
    }

    void OnCollisionEnter()
    {
        //GetComponent<AudioSource>().Play();
    }

    void OnTriggerEnter()
    {
        if (!scored && overHoop) {
            scored = true;
            shootCount++;
            AudioSource.PlayClipAtPoint(Applaud, transform.position);
        }
    }

    public void ResetScore() {
        shootCount = 0;
    }

}
