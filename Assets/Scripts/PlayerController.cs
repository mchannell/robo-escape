using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//add defects, add win condition, upload
public class PlayerController : MonoBehaviour
{
    public enum AIType {
        Aggressor, Bully, Lost, Random, Worker, ShutDown
    };
    bool aggressorTakeAction = true;
    public int workerNumBoxMoves;
    public bool workerJustFound;
    public bool workerFoundRecently;
    public int initWorkerNumBoxMoves;
    public bool movementBlocked;
    private GameObject workerTargetBox;
    private bool workerBoxHasBeenFound;
    public AIType aiType;
    public bool destroysOnAction;
    public LayerMask blocksMovement;
    public LayerMask immovable;
    public LayerMask robotLayer;
    public LayerMask endTrigger;
    public LayerMask keyLayer;
    public LayerMask lockLayer;
    public int initTurnsUntilShutdown;
    public int turnsUntilShutdown;
    public bool shutsDownAfterNumberOfTurns;
    public bool diesAfterNumberOfActions;
    public int numActionsUntilDeath;
    public bool hasReverseControls;
    public bool isControlledByPlayer;
    private float waitTime = .1f;
    private float timer = 0f;
    private float standardMoveDistance = 0.5f;
    float horizontalMove = 0f;
    float verticalMove = 0f;
    bool isPushPressed;
    bool isInteractPressed;
    bool isShutDownPressed;
    bool isButtonPressed;
    private bool hasKey = false;
    public MovementType movementType = MovementType.Queen;
    public enum MovementType {
        Rook, Bishop, Queen
    }
    public enum Direction {
        Left, UpLeft, DownLeft, Right, UpRight, DownRight, Up, Down
    }
    public Direction lookDir = Direction.Up;
    void Start()
    {
        if(hasReverseControls) {
            standardMoveDistance *= -1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isControlledByPlayer) {
            horizontalMove = Input.GetAxisRaw("Horizontal");
            verticalMove = Input.GetAxisRaw("Vertical");
            isPushPressed = Input.GetButton("Push");
            isInteractPressed = Input.GetButton("Interact");
            isShutDownPressed = Input.GetButton("ShutDown");
        }
        else if(aiType != AIType.ShutDown) {
            isButtonPressed = Input.GetButton("Push") || Input.GetButton("Interact") || Input.GetButton("ShutDown") || Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        }
    }

    void FixedUpdate () {
        if (Physics2D.OverlapCircle(transform.position, .1f, endTrigger)) {
            ScoreScript.scoreVal++;
            Destroy(gameObject);
        }

        if (timer > waitTime)
        {
            if(!isControlledByPlayer && isButtonPressed) {
                DetermineAIInput();
                RobotMovement();
                ClearInput();
            }
            else {
                RobotMovement();
            }
        }
        else {
            if(aiType != AIType.ShutDown || isControlledByPlayer) {
                timer += Time.fixedDeltaTime;
            }
        }
    }

    void RobotMovement() {
        bool resetTimer = true;
        switch ((movementType, horizontalMove, verticalMove, lookDir)) {
            case (_, 0f, 0f, _):
                resetTimer = CheckForOtherInput();
                if(resetTimer) {
                    if(diesAfterNumberOfActions) {
                        numActionsUntilDeath--;
                        if(numActionsUntilDeath <= 0) {
                            Destroy(gameObject);
                        }
                    }
                }
                break;
            case (MovementType.Queen, 1f, 1f, Direction.UpRight):
            case (MovementType.Queen, 1f, 0f, Direction.Right):
            case (MovementType.Queen, 1f, -1f, Direction.DownRight):
            case (MovementType.Queen, 0f, 1f, Direction.Up):
            case (MovementType.Queen, 0f, -1f, Direction.Down):
            case (MovementType.Queen, -1f, 1f, Direction.UpLeft):
            case (MovementType.Queen, -1f, 0f, Direction.Left):
            case (MovementType.Queen, -1f, -1f, Direction.DownLeft):
                if(!Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | immovable | lockLayer | robotLayer)) {
                    transform.position += new Vector3(standardMoveDistance * horizontalMove, standardMoveDistance * verticalMove, 0f);
                    Collider2D hitCollider;
                    hitCollider = Physics2D.OverlapCircle(transform.position, .1f, keyLayer, -Mathf.Infinity, Mathf.Infinity);
                    if(hitCollider != null) {
                        hasKey = true;
                        Destroy(hitCollider.gameObject);
                    }
                }
                break;
            case (MovementType.Queen, _, _, _):
                lookDir = VectorToDirection(horizontalMove, verticalMove);
                transform.GetChild(0).transform.position = transform.position + new Vector3(standardMoveDistance * horizontalMove, standardMoveDistance * verticalMove, 0f);
                break;
            case (MovementType.Rook, 1f, _, Direction.Right):
            case (MovementType.Rook, -1f, _, Direction.Left):
                if(!Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | immovable | lockLayer | robotLayer)) {
                    transform.position += new Vector3(standardMoveDistance * horizontalMove, 0f, 0f);
                    Collider2D hitCollider;
                    hitCollider = Physics2D.OverlapCircle(transform.position, .1f, keyLayer, -Mathf.Infinity, Mathf.Infinity);
                    if(hitCollider != null) {
                        hasKey = true;
                        Destroy(hitCollider.gameObject);
                    }
                }
                break;
            case (MovementType.Rook, _, 1f, Direction.Up):
            case (MovementType.Rook, _, -1f, Direction.Down):
                if(!Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | immovable | lockLayer | robotLayer)) {
                    transform.position += new Vector3(0f, standardMoveDistance * verticalMove, 0f);
                    Collider2D hitCollider;
                    hitCollider = Physics2D.OverlapCircle(transform.position, .1f, keyLayer, -Mathf.Infinity, Mathf.Infinity);
                    if(hitCollider != null) {
                        hasKey = true;
                        Destroy(hitCollider.gameObject);
                    }
                }
                break;
            case (MovementType.Rook, _, _, _):
                if (horizontalMove == 0f || ((lookDir == Direction.Up || lookDir == Direction.Down) && verticalMove != 0f)) {
                    lookDir = VectorToDirection(0f, verticalMove);
                    transform.GetChild(0).transform.position = transform.position + new Vector3(0f, standardMoveDistance * verticalMove, 0f);
                }
                else {
                    lookDir = VectorToDirection(horizontalMove, 0f);
                    transform.GetChild(0).transform.position = transform.position + new Vector3(standardMoveDistance * horizontalMove, 0f, 0f);
                }
                break;
            case (MovementType.Bishop, 1f, _, Direction.DownRight):
            case (MovementType.Bishop, -1f, _, Direction.UpLeft):
                if(!Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | immovable | lockLayer | robotLayer)) {
                    transform.position += new Vector3(standardMoveDistance * horizontalMove, -standardMoveDistance * horizontalMove, 0f);
                    Collider2D hitCollider;
                    hitCollider = Physics2D.OverlapCircle(transform.position, .1f, keyLayer, -Mathf.Infinity, Mathf.Infinity);
                    if(hitCollider != null) {
                        hasKey = true;
                        Destroy(hitCollider.gameObject);
                    }
                }
                break;
            case (MovementType.Bishop, _, 1f, Direction.UpRight):
            case (MovementType.Bishop, _, -1f, Direction.DownLeft):
                if(!Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | immovable | lockLayer | robotLayer)) {
                    transform.position += new Vector3(standardMoveDistance * verticalMove, standardMoveDistance * verticalMove, 0f);
                    Collider2D hitCollider;
                    hitCollider = Physics2D.OverlapCircle(transform.position, .1f, keyLayer, -Mathf.Infinity, Mathf.Infinity);
                    if(hitCollider != null) {
                        hasKey = true;
                        Destroy(hitCollider.gameObject);
                    }
                }
                break;
            case (MovementType.Bishop, _, _, _):
                if (horizontalMove == 0f || ((lookDir == Direction.UpRight || lookDir == Direction.DownLeft) && verticalMove != 0f)) {
                    lookDir = VectorToDirection(verticalMove, verticalMove);
                    transform.GetChild(0).transform.position = transform.position + new Vector3(standardMoveDistance * verticalMove, standardMoveDistance * verticalMove, 0f);
                }
                else {
                    lookDir = VectorToDirection(horizontalMove, -horizontalMove);
                    transform.GetChild(0).transform.position = transform.position + new Vector3(standardMoveDistance * horizontalMove, -standardMoveDistance * horizontalMove, 0f);
                }
                break;
            }
        if(resetTimer) {
            if(isControlledByPlayer && shutsDownAfterNumberOfTurns) {
                turnsUntilShutdown--;
                if(turnsUntilShutdown <= 0) {
                    isControlledByPlayer = false;
                    ClearInput();
                }
            }
            timer = timer - waitTime;
        }
    }
    Vector3 DirectionToVector(Direction dir) {
        return dir switch {
            Direction.UpRight => new Vector3(standardMoveDistance, standardMoveDistance, 0f),
            Direction.Right => new Vector3(standardMoveDistance, 0f, 0f),
            Direction.DownRight => new Vector3(standardMoveDistance, -standardMoveDistance, 0f),
            Direction.Up => new Vector3(0f, standardMoveDistance, 0f),
            Direction.Down => new Vector3(0f, -standardMoveDistance, 0f),
            Direction.UpLeft => new Vector3(-standardMoveDistance, standardMoveDistance, 0f),
            Direction.Left => new Vector3(-standardMoveDistance, 0f, 0f),
            Direction.DownLeft => new Vector3(-standardMoveDistance, -standardMoveDistance, 0f),
            _ => new Vector3(0f, 0f, 0f)
        };
    }
    Direction VectorToDirection(float x, float y) {
        return (x,y) switch {
            (1f, 1f) => Direction.UpRight,
            (1f, 0f) => Direction.Right,
            (1f, -1f) => Direction.DownRight,
            (0f, 1f) => Direction.Up,
            (0f, -1f) => Direction.Down,
            (-1f, 1f) => Direction.UpLeft,
            (-1f, 0f) => Direction.Left,
            (-1f, -1f) => Direction.DownLeft,
            _ => lookDir
        };
    }
    bool CheckForOtherInput() {
        bool returnVal = false;
        if(isPushPressed) {
            Collider2D hitCollider;
            hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement | robotLayer, -Mathf.Infinity, Mathf.Infinity);
            if(destroysOnAction) {
                if(hitCollider != null) {
                    Destroy(hitCollider.gameObject);
                }
            }
            if(hitCollider != null) {
                Vector3 targetPosition = hitCollider.transform.position + DirectionToVector(lookDir);
                if(!Physics2D.OverlapCircle(targetPosition, .1f, blocksMovement | robotLayer | immovable | lockLayer)) {
                    hitCollider.transform.position = targetPosition;
                }
            }
            returnVal = true;
        }
        else if(isInteractPressed) {
            Collider2D hitCollider;
            hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer, -Mathf.Infinity, Mathf.Infinity);
            if(destroysOnAction) {
                if(hitCollider == null) {
                    hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement, -Mathf.Infinity, Mathf.Infinity);
                }
                if(hitCollider != null) {
                    Destroy(hitCollider.gameObject);
                }
            }
            else if(hitCollider != null) {
                PlayerController robot = hitCollider.gameObject.GetComponent<PlayerController>();
                robot.isControlledByPlayer = true;
                robot.turnsUntilShutdown = robot.initTurnsUntilShutdown;
            }
            else if (hasKey) {
                hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, lockLayer, -Mathf.Infinity, Mathf.Infinity);
                if(hitCollider != null) {
                    Destroy(hitCollider.gameObject);
                    hasKey = false;
                }
            }
            returnVal = true;
        }
        else if(isShutDownPressed) {
            Collider2D hitCollider;
            hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer, -Mathf.Infinity, Mathf.Infinity);
            if(destroysOnAction) {
                if(hitCollider == null) {
                    hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement, -Mathf.Infinity, Mathf.Infinity);
                }
                if(hitCollider != null) {
                    Destroy(hitCollider.gameObject);
                }
            }
            else if(hitCollider != null) {
                hitCollider.gameObject.GetComponent<PlayerController>().isControlledByPlayer = false;
                hitCollider.gameObject.GetComponent<PlayerController>().ClearInput();
            }
            returnVal = true;
        }
        return returnVal;
    }

/*      AI bad. Generally just use AIType.ShutDown and ignore this section of code, but maybe revisit later.
        Need to fix layermask stuff to make it more clean and make sure AI can't do broken things, but just leave it for now since I'm not really using it anyway.
        Also need to make sure this stuff functions withthe new different movement types now that those are in.
        I really just need to completely rewrite it if I ever wanted to try to do something with this again, but that's fine.
*/

    void DetermineAIInput() {
        switch(aiType) {
            case AIType.Aggressor:
                ExecuteAggressorBehavior();
                break;
            case AIType.Bully:
                ExecuteBullyBehavior();
                break;
            case AIType.Lost:
                ExecuteLostBehavior();
                break;
            case AIType.Random:
                ExecuteRandomBehavior();
                break;
            case AIType.Worker:
                ExecuteWorkerBehavior();
                break;
            default:
                break;

        }
    }
    public void ClearInput() {
        verticalMove = 0f;
        horizontalMove = 0f;
        isInteractPressed = false;
        isShutDownPressed = false;
        isPushPressed = false;
    }
    void ExecuteAggressorBehavior() {
        if (aggressorTakeAction) {
            if (Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer)) {
                isShutDownPressed = true;
            } else {
                ChaseRobot(true);
            }
            aggressorTakeAction = false;
        }
        else {
            aggressorTakeAction = true;
        }
    }
    void ExecuteBullyBehavior() {
        if (Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer)) {
            isPushPressed = true;
        } else {
            ChaseRobot(false);
        }
    }
    void ExecuteLostBehavior() {
        Vector3 vector = DirectionToVector(lookDir);
        if (Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer) || Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement)) {
            Vector3 newVector = GetClockwiseVector(vector);
            horizontalMove = newVector.x;
            verticalMove = newVector.y;
        }
        else {
            horizontalMove = vector.x;
            verticalMove = vector.y;
        }
    }
    void ExecuteRandomBehavior() {
        //take random action
    }
    void ExecuteWorkerBehavior() {
        if(workerTargetBox == null) {
            GameObject[] boxes = (GameObject[]) GameObject.FindObjectsOfType(typeof(GameObject));
            float maxDistance = -Mathf.Infinity;
            Transform maxTransform = null;
            foreach (GameObject box in boxes) {
                if(box.name.Contains("box")) {
                    float dist = Vector3.Distance(box.transform.position, transform.position);
                    if (dist > maxDistance) {
                        maxTransform = box.transform;
                        maxDistance = dist;
                        workerTargetBox = box;
                    }
                }
            }
            if (maxTransform != null) {
                ChaseTransform(maxTransform);
            }
        }
        else {
            Collider2D hitCollider;
            hitCollider = Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement, -Mathf.Infinity, Mathf.Infinity);
            if(hitCollider != null && hitCollider.gameObject == workerTargetBox && !workerBoxHasBeenFound) {
                workerBoxHasBeenFound = true;
                workerJustFound = true;
                workerNumBoxMoves = initWorkerNumBoxMoves;
                horizontalMove = GetClockwiseVector(DirectionToVector(lookDir)).x;
                verticalMove = GetClockwiseVector(DirectionToVector(lookDir)).y;
            }
            else if(!workerBoxHasBeenFound) {
                ChaseTransform(workerTargetBox.transform);
            }
            else if(workerJustFound) {
                horizontalMove = DirectionToVector(lookDir).x * (1/standardMoveDistance);
                verticalMove = DirectionToVector(lookDir).y * (1/standardMoveDistance);
                workerJustFound = false;
                workerFoundRecently = true;
            }
            else if(workerFoundRecently) {
                horizontalMove = GetCounterClockwiseVector(DirectionToVector(lookDir)).x;
                verticalMove = GetCounterClockwiseVector(DirectionToVector(lookDir)).y;
                workerFoundRecently = false;
            }
            else if (workerNumBoxMoves == initWorkerNumBoxMoves) {
                ChaseTransform(workerTargetBox.transform);
                workerNumBoxMoves--;
            }
            else {
                if (workerNumBoxMoves <= 0) {
                    workerTargetBox = null;
                    workerBoxHasBeenFound = false;
                }
                else if (Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement)) {
                    isPushPressed = true;
                    workerNumBoxMoves--;
                }
                else {
                    ChaseTransform(workerTargetBox.transform);
                }
            }
        }
    }
    void ChaseRobot(bool chaseOnlyIfControlledByPlayer) {
        PlayerController[] robots = GameObject.FindObjectsOfType<PlayerController>();
        float minDistance = Mathf.Infinity;
        Transform minTransform = null;
        foreach (PlayerController robot in robots) {
            if(!chaseOnlyIfControlledByPlayer || robot.isControlledByPlayer) {
                float dist = Vector3.Distance(robot.transform.position, transform.position);
                if (dist < minDistance && dist != 0f) {
                    minTransform = robot.transform;
                    minDistance = dist;
                }
            }
        }
        if (minTransform != null) {
            ChaseTransform(minTransform);
        }
    }
    void ChaseTransform(Transform transformParam) {
        if(movementBlocked) {
            horizontalMove = DirectionToVector(lookDir).x * (1/standardMoveDistance);
            Debug.Log(horizontalMove);
            verticalMove = DirectionToVector(lookDir).y * (1/standardMoveDistance);
            movementBlocked = false;
        }
        else if (Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, robotLayer) || Physics2D.OverlapCircle(transform.GetChild(0).transform.position, .1f, blocksMovement)) {
            Vector3 clockwiseVector = GetClockwiseVector(transform.GetChild(0).transform.position);
            Vector3 counterClockwiseVector = GetCounterClockwiseVector(transform.GetChild(0).transform.position);
            if (Physics2D.OverlapCircle(clockwiseVector, .1f, robotLayer) || Physics2D.OverlapCircle(clockwiseVector, .1f, blocksMovement)) {
                if (Physics2D.OverlapCircle(counterClockwiseVector, .1f, robotLayer) || Physics2D.OverlapCircle(counterClockwiseVector, .1f, blocksMovement)) {
                        isPushPressed = true;
                }
                else {
                    horizontalMove = counterClockwiseVector.x;
                    verticalMove = counterClockwiseVector.y;
                    movementBlocked = true;
                }
            }
            else {
                horizontalMove = clockwiseVector.x;
                verticalMove = clockwiseVector.y;
                movementBlocked = true;
            }
        }
        if (transform.position.x > transformParam.position.x) {
            horizontalMove = -1f;
        }
        else if (transform.position.x < transformParam.position.x) {
            horizontalMove = 1f;
        }
        if (transform.position.y > transformParam.position.y) {
            verticalMove = -1f;
        }
        else if (transform.position.y < transformParam.position.y) {
            verticalMove = 1f;
        }
    }
    Vector3 GetClockwiseVector(Vector3 vector) {
        float xCheck = 0;
        float yCheck = 0;
        if (vector.x > 0) {
            xCheck = 1;
        }
        else if (vector.x < 0) {
            xCheck = -1;
        }
        if (vector.y > 0) {
            yCheck = 1;
        }
        else if (vector.y < 0) {
            yCheck = -1;
        }
        return (
                (xCheck, yCheck) switch {
                    (1f, 1f) => new Vector3(1f, 0f, 0f),
                    (1f, 0f) => new Vector3(1f, -1f, 0f),
                    (1f, -1f) => new Vector3(0f, -1f, 0f),
                    (0f, 1f) => new Vector3(1f, 1f, 0f),
                    (0f, -1f) => new Vector3(-1f, -1f, 0f),
                    (-1f, 1f) => new Vector3(0f, 1f, 0f),
                    (-1f, 0f) => new Vector3(-1f, 1f, 0f),
                    (-1f, -1f) => new Vector3(-1f, 0f, 0f),
                    _ => new Vector3(0f, 0f, 0f)
                }
            );
    }
    Vector3 GetCounterClockwiseVector(Vector3 vector) {
        float xCheck = 0;
        float yCheck = 0;
        if (vector.x > 0) {
            xCheck = 1;
        }
        else if (vector.x < 0) {
            xCheck = -1;
        }
        if (vector.y > 0) {
            yCheck = 1;
        }
        else if (vector.y < 0) {
            yCheck = -1;
        }
        return (
            (xCheck, yCheck) switch {
                (1f, 1f) => new Vector3(0f, 1f, 0f),
                (1f, 0f) => new Vector3(1f, 1f, 0f),
                (1f, -1f) => new Vector3(1f, 0f, 0f),
                (0f, 1f) => new Vector3(-1f, 1f, 0f),
                (0f, -1f) => new Vector3(1f, -1f, 0f),
                (-1f, 1f) => new Vector3(-1f, 0f, 0f),
                (-1f, 0f) => new Vector3(-1f, -1f, 0f),
                (-1f, -1f) => new Vector3(0f, -1f, 0f),
                _ => new Vector3(0f, 0f, 0f)
            });
    }
}