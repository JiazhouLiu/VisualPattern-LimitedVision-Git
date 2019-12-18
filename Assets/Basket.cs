using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public GameObject score;
    public AudioClip basket;

    void OnCollisionEnter()
    {
        GetComponent<AudioSource>().Play();
    }

    void OnTriggerEnter()
    {
        AudioSource.PlayClipAtPoint(basket, transform.position);
    }
}
