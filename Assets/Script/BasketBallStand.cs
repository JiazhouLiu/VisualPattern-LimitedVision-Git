using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketBallStand : MonoBehaviour
{
    private void Update()
    {
        if (GameObject.Find("Ball").transform.position.y > 1.2f) {    
            transform.gameObject.SetActive(false);
        }
    }

}
