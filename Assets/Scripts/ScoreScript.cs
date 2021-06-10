using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public static int scoreVal = 0;
    Text score;
    // Start is called before the first frame update
    void Start()
    {
        score = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindObjectsOfType<PlayerController>().Length == 0) {
            score.text = ("You Win!");
        }
        else {
            score.text = "Capture Robots with 'e' to control them and use them to escape.\nNumber of escaped robots: " + scoreVal;
        }
    }
}
