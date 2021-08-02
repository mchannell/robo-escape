using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    private bool isButtonPressed;
    private float waitTime = .1f;
    private float timer = 0f;
    public static int scoreVal = 0;
    private int moveCount = 0;
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
        isButtonPressed = Input.GetButton("Push") || Input.GetButton("Interact") || Input.GetButton("ShutDown") || Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        if (Input.GetKeyDown("r")) {
            scoreVal = 0;
            moveCount = 0;
            Application.LoadLevel (Application.loadedLevel);
        }
        if (GameObject.FindObjectsOfType<PlayerController>().Length == 0) {
            score.text = ("You Win!\n\nTotal Moves Taken: " + moveCount + "\n\nTry again to see if you can complete the level in fewer moves!");
        }
        else {
            score.text = "WASD to move.\n\nAll actions take effect on highlighted squares.\n\n'f' to push moveable objects (brown boxes and robots).\n\n" +
            "'e' to interact (gain control of robot or open door with key).\n\n'q' to lose control of robot.\n\n" +
            "'r' to restart\n\n<color=green>green</color> = orthogonal movement / normal actions.\n\n<color=blue>blue</color> = diagonal movement / normal actions.\n\n" +
            "<color=yellow>yellow</color> = orthogonal movement / all actions instead delete movable objects. Self destructs after 2 actions.\n\nMove Counter: " + moveCount;
        }
    }
    void FixedUpdate() {
        
        Debug.Log(timer);
        if (timer > waitTime)
        {
            if(isButtonPressed) {
                ++moveCount;
                timer = timer - waitTime;
            }
        }
        else {
            timer += Time.fixedDeltaTime;
        }
    }
}
