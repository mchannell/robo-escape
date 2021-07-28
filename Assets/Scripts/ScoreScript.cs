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
        // GameObject[] objectsInGame = (GameObject[]) GameObject.FindObjectsOfType(typeof(GameObject));
        // foreach (GameObject objectInGame in objectsInGame) {
        //     float easierRoundingNumX = (objectInGame.transform.position.x + .25f) * 2f;
        //     float easierRoundingNumY = (objectInGame.transform.position.y + .25f) * 2f;
        //     objectInGame.transform.position = new Vector3(Mathf.Round(easierRoundingNumX) / 2f - .25f, Mathf.Round(easierRoundingNumY) / 2f - .25f, 0f);
        // }
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
