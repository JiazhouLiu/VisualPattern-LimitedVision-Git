using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public AudioClip basket;

    void OnCollisionEnter()
    {
        GetComponent<AudioSource>().Play();
    }

    void OnTriggerEnter()
    {
        Debug.Log("Yes");
        AudioSource.PlayClipAtPoint(basket, transform.position);
    }
}
