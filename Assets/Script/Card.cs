using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public bool flipped = false;
    public bool interactable = false;
    public bool filled = false;
    public bool rotating = false;

    public bool seen = false;
    public bool selected = false;

    public bool seenLogged = false;
    public bool selectLogged = false;

    private Transform[] borders;

    private ExperimentManager em;

    private void Start()
    {
        if (GameObject.Find("ExperimentManager") != null) {
            em = GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>();
        }
        borders = new Transform[4]{
            transform.GetChild(0).GetChild(0).GetChild(2),
            transform.GetChild(0).GetChild(0).GetChild(3),
            transform.GetChild(0).GetChild(0).GetChild(4),
            transform.GetChild(0).GetChild(0).GetChild(5)
        };
    }

    private void Update()
    {
        if (em.gameState == GameState.ShowPattern) {
            if (filled && seen && !selected)
            {
                foreach (Transform t in borders)
                {
                    t.GetComponent<Image>().color = Color.yellow;
                }
            }
            else if (filled && !seen && selected)
            {
                foreach (Transform t in borders)
                {
                    t.GetComponent<Image>().color = Color.blue;
                }
            }
            else if (filled && seen && selected)
            {
                foreach (Transform t in borders)
                {
                    t.GetComponent<Image>().color = Color.green;
                }
            }
        }
        else
        {
            foreach (Transform t in borders)
            {
                t.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
