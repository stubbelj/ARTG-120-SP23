using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Player : MonoBehaviour
{
    Vector2 moveSpeed = new Vector2(100, 100);
    Vector2 maxSpeed = new Vector2(15f, 15f);
    float friction = 1f;
    float jumpForce = 1400f;
    [SerializeField]
    bool isGrounded = false;
    int jumps = 0;
    int recoveries = 0;
    bool playerNumber;
    float percent = 0;
    bool stunned = false;
    TMP_Text percentText;

    string actionInput = null;
    string dirInput = null;
    public Animator anim;
    //this is an animator component on this object, which runs animation
    public string currAnimState;
    //current animation
    public string currState = null;
    Coroutine currActionCoroutine = null;

    List<int> attackedBy = new List<int>();
    List<int> damagedBy = new List<int>();

    GameManager gameManager;
    public SpriteRenderer sr;
    Rigidbody2D rb;
    Dictionary<string, string> controlScheme;
    bool controlSchemeController = false;
    public static Dictionary<string, string>[] controlSchemeData = new Dictionary<string, string>[]{
        //keyboard 1
        new Dictionary<string, string>{
            {"w", "up"},
            {"s", "down"},
            {"a", "left"},
            {"d", "right"},
            {"v", "attack"},
            {"b", "jump"},
            {"n", "block"},
            {"m", "grab"}
        },
        //keyboard 2
        new Dictionary<string, string>{
            {"up", "up"},
            {"down", "down"},
            {"left", "left"},
            {"right", "right"},
            {"u", "attack"},
            {"i", "jump"},
            {"o", "block"},
            {"p", "grab"}
        },
        //controller inputs for verti/horiz are mapped differently and don't use dict values
        //xbox controller
        new Dictionary<string, string>{
            {"Vertical_Xbox_1", "up"},
            {"Horizontal_Xbox_1", "left"},
            {"Attack_Xbox_1", "attack"},
            {"Jump_Xbox_1", "jump"},
            {"Block_Xbox_1", "block"},
            {"Grab_Xbox_1", "grab"}
        },

        new Dictionary<string, string>{
            {"Vertical_Xbox_2", "up"},
            {"Horizontal_Xbox_2", "left"},
            {"Attack_Xbox_2", "attack"},
            {"Jump_Xbox_2", "jump"},
            {"Block_Xbox_2", "block"},
            {"Grab_Xbox_2", "grab"}
        }
    };
    public static int[] activeControlSchemes = new int[2];

    Dictionary<string, bool>[] inputs = new Dictionary<string, bool>[]{
        //player 1
        new Dictionary<string, bool>{
            //single inputs
            {"up", false},
            {"down", false},
            {"left", false},
            {"right", false},
            {"attack", false},
            {"block", false},
            {"grab", false},
            {"jump", false},
            //composite inputs
            {"upAttack", false},
            {"downAttack", false},
            {"leftAttack", false},
            {"rightAttack", false},
        },
        //player 2
        new Dictionary<string, bool>{
            //single inputs
            {"up", false},
            {"down", false},
            {"left", false},
            {"right", false},
            {"attack", false},
            {"block", false},
            {"grab", false},
            {"jump", false},
            //composite inputs
            {"upAttack", false},
            {"downAttack", false},
            {"leftAttack", false},
            {"rightAttack", false},
        }
    };
    
    void Awake() {
        gameManager = GameManager.inst;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        LoadAttackData();
    }
    
    void Update() {

        if (controlSchemeController) {
            string schemeAppend;
            if (!playerNumber) {
                schemeAppend = gameManager.p1Scheme == 2 ? "_1" : "_2";
            } else {
                schemeAppend = gameManager.p2Scheme == 2 ? "_1" : "_2";
            }

            if (Input.GetAxis("Vertical_Xbox" + schemeAppend) == -1) {
                inputs[0]["up"] = true;
            } else if (Input.GetAxis("Vertical_Xbox" + schemeAppend) == 1) {
                inputs[0]["down"] = true;
            } else {
                inputs[0]["up"] = false;
                inputs[0]["down"] = false;
            }
            if (Input.GetAxis("Horizontal_Xbox" + schemeAppend) == 1) {
                inputs[0]["right"] = true;
            } else if (Input.GetAxis("Horizontal_Xbox" + schemeAppend) == -1) {
                inputs[0]["left"] = true;
            } else {
                inputs[0]["right"] = false;
                inputs[0]["left"] = false;
            }

            if (Input.GetButtonDown("Attack_Xbox" + schemeAppend)) {
                inputs[0]["attack"] = true;
            } else {
                inputs[0]["attack"] = false;
            }
            if (Input.GetButtonDown("Jump_Xbox" + schemeAppend)) {
                inputs[0]["jump"] = true;
            } else {
                inputs[0]["jump"] = false;
            }
            if (Input.GetButtonDown("Block_Xbox" + schemeAppend)) {
                inputs[0]["block"] = true;
            } else {
                inputs[0]["block"] = false;
            }
            if (Input.GetButtonDown("Grab_Xbox" + schemeAppend)) {
                inputs[0]["grab"] = true;
            } else {
                inputs[0]["grab"] = false;
            }
        } else {
            //store keys pressed this frame in inputs[0]
            foreach (string key in controlScheme.Keys) {
                if(Input.GetKey(key)) {
                    inputs[0][controlScheme[key]] = true;
                } else {
                    inputs[0][controlScheme[key]] = false;
                }
            }
        }
        


        ReduceInputs();

        ExecuteInputs();

        Collider2D groundCastOverlap = Physics2D.OverlapBox(transform.position + new Vector3(0, -2, 0), new Vector2(1, 3), 0, LayerMask.GetMask("Ground"));

        if (groundCastOverlap && groundCastOverlap.gameObject.tag == "Ground" && (Mathf.Abs(transform.position.y - (groundCastOverlap.gameObject.transform.position.y + groundCastOverlap.bounds.extents.y + groundCastOverlap.offset.y)) < 1.9f)) {
            isGrounded = true;
            if (jumps != 1 && currState != "jumpTakeoff" && currState != "jumpRising" && currState != "jumpFalling") {
                jumps = 1;
            }
            recoveries = 1;
            if(currState == "jumpFalling") {
                currState = "jumpLanding";
                StartCoroutine(JumpLanding());
            }
        } else {
            isGrounded = false;
            if (rb.velocity.y < -0.5f && new string[]{null, "jumpRising", "idle", "walking", "lookUp", "crouch"}.Contains(currState)) {
                currState = "jumpFalling";
                ChangeAnimationState("jumpFalling");
            }
        }

        //save input from this frame
        inputs[1] = inputs[0];
    }

    void ReduceInputs() {
        /*reduce conflicting inputs to valid inputs*/

        //left, right, up and down are non-simultaneous holds
        //all other actions are non-simultaneous presses

        if (stunned) {
            dirInput = null;
            actionInput = null;
            return;
        }

        dirInput = null;
        actionInput = null;
        List<string> dirKeys = new List<string>(inputs[0].Keys.ToList().GetRange(0, 4));
        List<string> actionKeys = new List<string>(inputs[0].Keys.ToList().GetRange(4, inputs[0].Count - 4));
        List<bool> actionVals = new List<bool>(inputs[0].Values.ToList().GetRange(4, inputs[0].Count - 4));
        //reduce directional inputs down to a single directional input
        //
        
        
        if (((List<bool>)inputs[0].Values.ToList().GetRange(0, 4)).Count(x => x) > 1) {
            //colliding dir inputs exist
            /*for (int i = 0; i < 4; i++) {
                if (dirVals[i] && inputs[1][dirKeys[i]]) {
                    //overlapping old inputs, erase from new inputs
                    inputs[0][dirKeys[i]] = false;
                }
            }*/
            //keep one of the dir inputs - they might still be colliding
            /*probably doesn't matter if some inputs hold precedence, but i selected the input to keep randomly anyway
            List<string> inputsToRemove = new List<string>{"up", "down", "left", "right"};
            inputsToRemove.RemoveAt(gameManager.rand.Next(0, 4));
            foreach (string inputName in inputsToRemove) {
                inputs[0][inputName] = false;
            }*/

            if (inputs[0]["up"]) {
                inputs[0]["down"] = inputs[0]["left"] = inputs[0]["right"] = false;
            } else if (inputs[0]["down"]) {
                inputs[0]["up"] = inputs[0]["left"] = inputs[0]["right"] = false;
            } else if (inputs[0]["left"]) {
                inputs[0]["down"] = inputs[0]["up"] = inputs[0]["right"] = false;
            } else {
                inputs[0]["down"] = inputs[0]["left"] = inputs[0]["up"] = false;
            }
        }

        for (int i = 0; i < 4; i++) {
            if (inputs[0].Values.ToList().GetRange(0, 4)[i]) {dirInput = dirKeys[i];}
        }
        //value that's actually being used in this implementation

        //reduce action inputs down to a single action input
        if (((List<bool>)inputs[0].Values.ToList().GetRange(4, inputs[0].Count - 4)).Count(x => x) > 1) {
            //colliding action inputs exist
            for (int i = 0; i < inputs[0].Count - 4; i++) {
                if (actionVals[i] && inputs[1][actionKeys[i]]) {
                    //overlapping old inputs, erase from new inputs
                    inputs[0][actionKeys[i]] = false;
                }
            }
            //keep one of the action inputs - they might still be colliding
            //probably doesn't matter if some inputs hold precedence, but i selected the input to keep randomly anyway
            List<string> inputsToRemove = new List<string>{"attack", "block", "grab", "jump"};
            inputsToRemove.RemoveAt(gameManager.rand.Next(0, 2));
            foreach (string inputName in inputsToRemove) {
                inputs[0][inputName] = false;
            }
        }

        for (int i = 0; i < inputs[0].Count - 4; i++) {
            if (actionVals[i]) { actionInput = actionKeys[i];}
        }
        //value that's actually being used in this implementation
    }

    void ExecuteInputs() {
        /*have the player perform actions based on the (reduced) input*/
        if (actionInput == null) {
            //player is walking
            if ((dirInput == "left" || dirInput == "right") && (new string[]{null, "idle", "walking", "lookUp", "crouch"}.Contains(currState))) {
                currState = "walking";
                float turnAroundFactor = 1;
                if ((dirInput == "right" && rb.velocity.x < 0) || (dirInput == "left" && rb.velocity.x > 0)) {
                    turnAroundFactor *= 2;
                    //helps players handle excessive momentum
                }
                rb.velocity += (Mathf.Abs(rb.velocity.x) < maxSpeed.x ? 1 : 0) * new Vector2((isGrounded ? 1 : 0.5f) * (dirInput == "right" ? 1 : -1) * moveSpeed.x * turnAroundFactor * Time.deltaTime, 0);
                transform.Find("Colliders").Find("WalkColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                ChangeAnimationState("walk");
                if ((sr.flipX && dirInput == "right") || (!sr.flipX && dirInput == "left")) {
                    sr.flipX = !sr.flipX;
                    foreach(Transform colliderParent in transform.Find("Colliders")) {
                        foreach(Transform frameParent in colliderParent) {
                            foreach(Transform box in frameParent) {
                                box.GetComponent<BoxCollider2D>().offset = new Vector2(-box.GetComponent<BoxCollider2D>().offset.x, box.GetComponent<BoxCollider2D>().offset.y);
                            }
                        }
                    }
                    transform.Find("GrabSnap").localPosition = new Vector3(-transform.Find("GrabSnap").localPosition.x, transform.Find("GrabSnap").localPosition.y, 0);
                }
            } else if (rb.velocity.y < -0.5f && !isGrounded && (new string[]{null, "idle", "walk", "jumpRising"}.Contains(currState))) {
                //falling
                currState = "jumpFalling";
                transform.Find("Colliders").Find("JumpFallingColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                ChangeAnimationState("jumpFalling");
            } else if (dirInput == "down" && isGrounded && (new string[]{null, "idle", "walking", "crouch"}.Contains(currState))) {
                //crouch
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "crouch";
                ChangeAnimationState("crouch");
            } else if (dirInput == "up" && isGrounded && (new string[]{null, "idle", "walking", "lookUp"}.Contains(currState))) {
                //look up
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "lookUp";
                ChangeAnimationState("lookUp");
            } else if (dirInput == null && (new string[]{null, "idle", "walking", "lookUp", "crouch"}.Contains(currState))) {
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            }

            if ((dirInput == "left" || dirInput == "right") && !isGrounded) {
                //turning midAir
                float turnAroundFactor = 1;
                if ((dirInput == "right" && rb.velocity.x < 0) || (dirInput == "left" && rb.velocity.x > 0)) {
                    turnAroundFactor *= 2;
                    //helps players handle excessive momentum
                }
                rb.velocity += (Mathf.Abs(rb.velocity.x) < maxSpeed.x ? 1 : 0) * new Vector2((isGrounded ? 1 : 0.5f) * (dirInput == "right" ? 1 : -1) * moveSpeed.x * turnAroundFactor * Time.deltaTime, 0);
                if ((sr.flipX && dirInput == "right") || (!sr.flipX && dirInput == "left")) {
                    sr.flipX = !sr.flipX;
                    foreach(Transform colliderParent in transform.Find("Colliders")) {
                        foreach(Transform frameParent in colliderParent) {
                            foreach(Transform box in frameParent) {
                                box.GetComponent<BoxCollider2D>().offset = new Vector2(-box.GetComponent<BoxCollider2D>().offset.x, box.GetComponent<BoxCollider2D>().offset.y);
                            }
                        }
                    }
                    transform.Find("GrabSnap").localPosition = new Vector3(-transform.Find("GrabSnap").localPosition.x, transform.Find("GrabSnap").localPosition.y, 0);
                }
            }

        } else if (actionInput == "attack" && new string[]{null, "idle", "walking", "jumpTakeoff", "jumpRising", "jumpFalling"}.Contains(currState)) {
            if (isGrounded) {
                if (dirInput == "left" || dirInput == "right") {
                    //side and neutral attacks are the same right now
                    currActionCoroutine = StartCoroutine(SideAttack());
                } else if (dirInput == "up") {
                    currActionCoroutine = StartCoroutine(UpAttack());
                } else if (dirInput == "down") {
                    currActionCoroutine = StartCoroutine(DownAttack());
                } else if (dirInput == null) {
                    currActionCoroutine = StartCoroutine(SideAttack());
                }
            } else {
                //air attack
                if (dirInput == "left" || dirInput == "right") {
                    //side and neutral attacks are the same right now
                    currActionCoroutine = StartCoroutine(SideAttack());
                } else if (dirInput == "up") {
                    if (recoveries > 0) {
                        recoveries--;
                        currActionCoroutine = StartCoroutine(UpAirAttack());
                    }
                } else if (dirInput == "down") {
                    currActionCoroutine = StartCoroutine(DownAirAttack());
                } else if (dirInput == null) {
                    currActionCoroutine = StartCoroutine(SideAttack());
                }
            }
        } else if (actionInput == "jump") {
            if (jumps > 0) {
                jumps--;
                currState = "jumping";
                StartCoroutine(Jump());
            }
        } else if (actionInput == "block" && new string[]{null, "idle", "walking"}.Contains(currState)) {
            currActionCoroutine = StartCoroutine(Block());
        } else if (actionInput == "grab" && new string[]{null, "idle", "walking"}.Contains(currState)) {
            currActionCoroutine = StartCoroutine(Grab());
        } else if (actionInput == "attack" && new string[]{null, "lookUp", "crouch"}.Contains(currState)) {
            if (currState == "lookUp" && dirInput == "up") {
                currActionCoroutine = StartCoroutine(UpAttack());
            } else if (currState == "crouch" && dirInput == "down") {
                currActionCoroutine = StartCoroutine(DownAttack());
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D col) {
        //just for platforming and death box
        /*if (col.gameObject.tag == "Ground" && isGrounded == false) {
            Collider2D groundCastOverlap = Physics2D.OverlapBox(transform.position + new Vector3(0, -2, 0), new Vector2(1, 3), 0, LayerMask.GetMask("Ground"));
            if (groundCastOverlap) {
                if (groundCastOverlap.gameObject.tag == "Ground") {
                    isGrounded = true;
                    jumps = 1;
                    if (currState == "jumpFalling") {
                        currState = "jumpLanding";
                        StartCoroutine(JumpLanding());
                    }
                }
            }
        } else */
        if (col.gameObject.tag == "DeathBox") {
            Die();
        }
    }

    public void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "Ground" && isGrounded) {
            //isGrounded = false;
            //currState = "jumpFalling";
            //ChangeAnimationState("jumpFalling");
        }
    }

    bool jumpLock = false;
    IEnumerator Jump() {
        if (!jumpLock) {
            jumpLock = true;
            transform.Find("Colliders").Find("JumpTakeoffColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
            ChangeAnimationState("jumpTakeoff");
            StartCoroutine(ForceOverTime(new Vector3(0, jumpForce, 0), 0.1f));
            yield return new WaitForSeconds(0.125f);
            currState = "jumpRising";
            jumpLock = false;
        }
    }

    IEnumerator JumpLanding() {
        transform.Find("Colliders").Find("JumpLandingColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("jumpLanding");
        yield return new WaitForSeconds(0.083f);
        currState = null;
    }

    IEnumerator Block() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[5], 0.5f);
        //enables block collider and hides all other colliders 
        DisableHitHurtBoxes();
        currState = "blocking";
        stunned = true;
        ChangeAnimationState("block");
        transform.Find("Colliders").Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        transform.Find("Colliders").Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(.3f);
        transform.Find("Colliders").Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("Colliders").Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        stunned = false;
        currState = null;
    }

    public void EndBlock() {
        //cancels your block, for when another player hits your block
        StopCoroutine(currActionCoroutine);
        currActionCoroutine = null;
        transform.Find("Colliders").Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("Colliders").Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        stunned = false;
        currState = null;
    }

    public IEnumerator Grab() {
        //disables own hitboxes and enables special grab hitbox
        DisableHitBoxes();
        currState = "grabbing";
        ChangeAnimationState("grab");
        transform.Find("Colliders").Find("GrabColliders").Find("Frame0").Find("GrabHitbox").gameObject.GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.208f);
        transform.Find("Colliders").Find("GrabColliders").Find("Frame0").Find("GrabHitbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
    }

    void EndGrab() {
        if (currActionCoroutine != null) {
            StopCoroutine(currActionCoroutine);
        }
        transform.Find("Colliders").Find("GrabColliders").Find("Frame0").Find("GrabHitbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
    }

    public IEnumerator Throw(){
        //called on player that grabbed the other player
        currState = "throwing";
        gameManager.players[playerNumber ? 0 : 1].transform.position = transform.Find("GrabSnap").position;
        gameManager.players[playerNumber ? 0 : 1].sr.flipX = gameManager.players[playerNumber ? 0 : 1].transform.position.x > transform.position.x ? true : false;
        gameManager.players[playerNumber ? 0 : 1].rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.1f);
        //prevent player from pre-inputting by accident
        float grabTimeout = 3f;
        while (grabTimeout > 0) {
            gameManager.players[playerNumber ? 0 : 1].transform.position = transform.Find("GrabSnap").position;
            if (actionInput != null) {
                gameManager.players[playerNumber ? 0 : 1].EndThrown();
                StopCoroutine(currActionCoroutine);
                break;
            }
            if (dirInput != null) {
                if ((sr.flipX && dirInput == "right") || (!sr.flipX && dirInput == "left")) {
                    foreach (Player pl in gameManager.players) {
                        pl.sr.flipX = !pl.sr.flipX;
                        foreach(Transform colliderParent in pl.transform.Find("Colliders")) {
                            foreach(Transform frameParent in colliderParent) {
                                foreach(Transform box in frameParent) {
                                    box.GetComponent<BoxCollider2D>().offset = new Vector2(-box.GetComponent<BoxCollider2D>().offset.x, box.GetComponent<BoxCollider2D>().offset.y);
                                }
                            }
                        }
                        transform.Find("GrabSnap").localPosition = new Vector3(-transform.Find("GrabSnap").localPosition.x, transform.Find("GrabSnap").localPosition.y, 0);
                    }
                    gameManager.players[playerNumber ? 0 : 1].transform.position = new Vector3(transform.position.x + -(gameManager.players[playerNumber ? 0 : 1].transform.position.x - transform.position.x), gameManager.players[playerNumber ? 0 : 1].transform.position.y, 0);
                    gameManager.players[playerNumber ? 0 : 1].EndThrown();
                    break;
                }
            }
            yield return null;
            grabTimeout -= Time.deltaTime;
        }
        if (grabTimeout <= 0) {
            currState = null;
            gameManager.players[playerNumber ? 0 : 1].stunned = false;
            gameManager.players[playerNumber ? 0 : 1].currState = null;
        }
        ChangeAnimationState("throw");
        yield return new WaitForSeconds(0.2f);
        EndGrab();
    }

    public void BeginThrown() {
        //called on the player being grabbed
        DisableHitHurtBoxes();
        ChangeAnimationState("grabbed");
        if (currState == "blocking") {
            EndBlock();
        }
        stunned = true;
        currState = "thrown";
    }

    public void EndThrown() {
        AttackData grabAttackData = new AttackData(4, 0.0f, false, 1.22f, gameManager.players[playerNumber ? 1 : 0], AttackData.attackIdFlow++);
        TakeDamage(grabAttackData, (transform.position - gameManager.players[playerNumber ? 0 : 1].transform.position) / 2);
    }

    //All of these just toggle hitboxes for the frames of an attack
    IEnumerator SideAttack() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], 0.5f);
        currState = "attacking";
        ChangeAnimationState("sideSlash");
        yield return new WaitForSeconds(0.375f / 9);
        
        Transform prevFrame = null;
        foreach (Transform frame in transform.Find("Colliders").Find("SideAttackColliders")) {
            if (frame.Find("Hitbox") != null) {
                frame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hitbox") != null) {
                        prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                prevFrame = frame;
            }
            if (frame.Find("Hurtbox") != null) {
                frame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hurtbox") != null) {
                        prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.375f / 9);
        }
        if (prevFrame.Find("Hitbox") != null) {
                prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
            }
        if (prevFrame.Find("Hurtbox") != null) {
            prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        }

        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator DownAttack() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], 0.5f);
        currState = "attacking";
        ChangeAnimationState("downSlash");
        yield return new WaitForSeconds(0.208f / 8);
        
        Transform prevFrame = null;
        foreach (Transform frame in transform.Find("Colliders").Find("SideAttackColliders")) {
            if (frame.Find("Hitbox") != null) {
                frame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hitbox") != null) {
                        prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                prevFrame = frame;
            }
            if (frame.Find("Hurtbox") != null) {
                frame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hurtbox") != null) {
                        prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.208f / 8);
        }
        if (prevFrame.Find("Hitbox") != null) {
                prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
            }
        if (prevFrame.Find("Hurtbox") != null) {
            prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        }

        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator UpAttack() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], 0.5f);
        currState = "attacking";

       ChangeAnimationState("upSlash");
       Transform prevFrame = null;
        foreach (Transform frame in transform.Find("Colliders").Find("SideAttackColliders")) {
            if (frame.Find("Hitbox") != null) {
                frame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hitbox") != null) {
                        prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                prevFrame = frame;
            }
            if (frame.Find("Hurtbox") != null) {
                frame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hurtbox") != null) {
                        prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.375f / 8);
        }
        if (prevFrame.Find("Hitbox") != null) {
                prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
            }
        if (prevFrame.Find("Hurtbox") != null) {
            prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        }
        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator UpAirAttack() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], 0.5f);
        currState = "attacking";
        ChangeAnimationState("upAir");
        //StartCoroutine(SimpleLaunch(0.5f* Mathf.PI, new Vector3(0f * (sr.flipX ? -1 : 1), 30, 0)));
        yield return new WaitForSeconds(0.292f / 8);

        Transform prevFrame = null;
        foreach (Transform frame in transform.Find("Colliders").Find("SideAttackColliders")) {
            if (frame.Find("Hitbox") != null) {
                frame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hitbox") != null) {
                        prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                prevFrame = frame;
            }
            if (frame.Find("Hurtbox") != null) {
                frame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hurtbox") != null) {
                        prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.292f / 8);
        }
        if (prevFrame.Find("Hitbox") != null) {
                prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
            }
        if (prevFrame.Find("Hurtbox") != null) {
            prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        }
        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator DownAirAttack() {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], 0.5f);
        currState = "attacking";
        ChangeAnimationState("downAir");
        yield return new WaitForSeconds(0.292f / 8);

        Transform prevFrame = null;
        foreach (Transform frame in transform.Find("Colliders").Find("SideAttackColliders")) {
            if (frame.Find("Hitbox") != null) {
                frame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hitbox") != null) {
                        prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                prevFrame = frame;
            }
            if (frame.Find("Hurtbox") != null) {
                frame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                if (prevFrame != null) {
                    if (prevFrame.Find("Hurtbox") != null) {
                        prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.292f / 8);
        }
        if (prevFrame.Find("Hitbox") != null) {
                prevFrame.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
            }
        if (prevFrame.Find("Hurtbox") != null) {
            prevFrame.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        }
        currState = null;
        currActionCoroutine = null;
    }

    public void TakeDamage(AttackData aD, Vector3 contactPoint) {
        //called by hitboxes when they contact a hurtbox, adds invlun to attack instances and calls HitStunAndLaunch
        if ((!damagedBy.Contains(aD.damageInst)) && (aD.multiHit || !attackedBy.Contains(aD.attackId))) {
            stunned = true;
            currState = "hurt";
            DisableHitBoxes();
            ChangeAnimationState("hurt");
            //Vector3 forceVec = ((Vector3)transform.position - contactPoint).normalized;
            Vector3 forceVec = ((Vector3)transform.position - gameManager.players[playerNumber ? 0 : 1].transform.position).normalized;
            percent += aD.damage;
            percentText.text = percent.ToString();
            forceVec *= aD.damage * (350 + percent * 2);
            StartCoroutine(HitStunAndLaunch(0.1f, aD, forceVec));
        }
        damagedBy.Add(aD.damageInst);
        attackedBy.Add(aD.attackId);
        StartCoroutine(RefreshInvulnById(aD.damageInst, aD.attackId));
    }

    public IEnumerator HitStunAndLaunch(float mag, AttackData ad, Vector3 forceVec) {
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[4], 0.25f);
        //called when you take damage, makes you get stunned, shake a lil and then get launched
        Vector3 originPos = transform.position;
        if (currActionCoroutine != null) {
            StopCoroutine(currActionCoroutine);
            currActionCoroutine = null;
        }
        while(ad.hitStun > 0) {
            transform.position = new Vector3(originPos.x + (gameManager.rand.Next(-1, 1) * mag), originPos.y + (gameManager.rand.Next(-1, 1) * mag), originPos.z);
            ad.hitStun -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        rb.velocity = Vector3.zero;
        //rb.AddForce(forceVec);
        //StartCoroutine(KnockbackFriction(5f));
        StartCoroutine(SimpleLaunch(ad.launchAngle, forceVec));
        stunned = false;
    }

    public IEnumerator SimpleLaunch(float launchAngle, Vector3 forceVec) {
        //different launch for each attack
        float mag = forceVec.magnitude;
        float fx = forceVec.x;
        float fy = forceVec.y;
        float angle = launchAngle;

        //change angle of launch based on ad.launchAngle
        if(fx >= 0 && fy <= 0) {
            //lower right quadrant
            fx = Mathf.Abs(Mathf.Sin(angle) * mag);
            fy = -Mathf.Abs(Mathf.Cos(angle) * mag);
        } else if(fx <= 0 && fy <= 0) {
            //lower left quadrant
            fx = -Mathf.Abs(Mathf.Sin(angle) * mag);
            fy = -Mathf.Abs(Mathf.Cos(angle) * mag);
        }

        //if player is grounded, make them launch up
        if (isGrounded && fy < 0) {
            fy *= -1;
        }

        //apply force
        float launchDir = fx < 0 ? -1 : 1;
        rb.velocity = Vector3.zero;
        rb.AddForce(new Vector3(fx, fy, 0));
        float dfy = 0;
        //creates exponentially decreasing velocity
        while (rb.velocity.y > 0f) {
            print(fy);
            rb.AddForce(new Vector3(fx, fy, 0));
            fx += launchDir * Time.deltaTime;
            fy -= Time.deltaTime * dfy;
            dfy += 0.1f;
            yield return 0;
        }
        
        rb.velocity = new Vector3(rb.velocity.x, 0, 0);
        currState = null;
    }

    public IEnumerator KnockbackFriction(float time) {
        while (time > 0 && !isGrounded) {
            if (rb.velocity.x > 0) {
                rb.velocity -= new Vector2(0.05f * friction, 0);
            } else if (rb.velocity.x < 0) {
                rb.velocity += new Vector2(0.05f * friction, 0);
            }
            yield return 0;
            time -= Time.deltaTime;
        }
    }

    void DisableHitHurtBoxes() {
        if (currActionCoroutine != null) {
            StopCoroutine(currActionCoroutine);
        }
        /*disables all hit and hurt boxes, effectively making the player invincible and unable to deal damage. used by block
        because it does NOT DISABLE BLOCK HITBOX*/
        foreach(Transform colliderParent in transform.Find("Colliders")) {
            foreach(Transform frameParent in colliderParent) {
                foreach(Transform box in frameParent) {
                    if (box.tag == "Hitbox" || box.tag == "Hurtbox") {
                        box.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }
    }

    void DisableHitBoxes() {
        if (currActionCoroutine != null) {
            StopCoroutine(currActionCoroutine);
        }
        /*disables all hit boxes, effectively making the player unable to deal damage. used when player is hit*/
        foreach(Transform colliderParent in transform.Find("Colliders")) {
            foreach(Transform frameParent in colliderParent) {
                foreach(Transform box in frameParent) {
                    if (box.tag == "Hitbox") {
                        box.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }
    }

    IEnumerator RefreshInvulnById(int damageInst, int attackId) {
        //makes it so that you're invulnerable to a specific instnace of an attack (ex. THAT ONE TIME the player pressed attack) for a lil bit
        yield return new WaitForSeconds(0.15f);
        damagedBy.Remove(damageInst);
        attackedBy.Remove(attackId);
    }

    void LoadAttackData() {
        //initializes hitboxes with data about their respective attacks
        Transform sideAttack = transform.Find("Colliders").Find("SideAttackColliders");
        int sideAttackId = AttackData.attackIdFlow++;
        sideAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, false, 0.52f, this, sideAttackId);
        sideAttack.Find("Frame5").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(5f, 0.15f, false, 0.52f, this, sideAttackId);

        Transform upAttack = transform.Find("Colliders").Find("UpAttackColliders");
        int upAttackId = AttackData.attackIdFlow++;
        upAttack.Find("Frame3").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(2f, 0.15f, true, 1.22f, this, upAttackId);
        upAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, 1.22f, this, upAttackId);
        upAttack.Find("Frame5").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(5f, 0.15f, true, 1.22f, this, upAttackId);

        Transform downAttack = transform.Find("Colliders").Find("DownAttackColliders");
        int downAttackId = AttackData.attackIdFlow++;
        downAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(2f, 0.15f, true, 0.17f, this, downAttackId);
        downAttack.Find("Frame3").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, 0.17f, this, downAttackId);
        downAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(5f, 0.15f, true, 0.17f, this, downAttackId);

        Transform upAirAttack = transform.Find("Colliders").Find("UpAirAttackColliders");
        int upAirAttackId = AttackData.attackIdFlow++;
        upAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, 1.57f, this, upAirAttackId);
        upAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(7f, 0.15f, true, 1.57f, this, upAirAttackId);

        Transform downAirAttack = transform.Find("Colliders").Find("DownAirAttackColliders");
        int downAirAttackId = AttackData.attackIdFlow++;
        downAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, 0f, this, downAirAttackId);
        downAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(7f, 0.15f, true, 0f, this, downAirAttackId);
    }

    IEnumerator ForceOverTime(Vector3 force, float time) {
        //applies a force over time, used for making the jump feel right
        while(time > 0) {
            rb.AddForce(force * (0.1f / time));
            yield return new WaitForSeconds(0.1f);
            time -= 0.1f;
        }
    }

    public void Init(bool newPlayerNumber, int newControlScheme, TMP_Text newPercentText, bool isUsingController) {
        print(newControlScheme);
        //initializer bc monobehaviours don't really get constructors and Start() isn't safe
        playerNumber = newPlayerNumber;
        controlScheme = controlSchemeData[newControlScheme];
        percentText = newPercentText;
        controlSchemeController = isUsingController;

        EnablePerks();
    }

    void EnablePerks() {
        foreach(string perkName in gameManager.playerPerks[playerNumber ? 1 : 0]) {
            switch(perkName) {
                case "superSpeed":
                    moveSpeed *= 2;
                    maxSpeed *= 2;
                    break;
                case "heavyAttacks":
                    break;
                case "blinkAttacks":
                    break;
                default:
                    break;
            }
        }
    }

    void Die() {
        //ask gameManager to reset the game
        gameManager.AwardPoint(!playerNumber);
    }

    public bool AnimatorIsPlaying() {
        /*check if animation is playing*/
        return anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public bool AnimatorIsPlaying(string stateName) {
        stateName += playerNumber ? "_p2" : "_p1";
        /*check if a specific animation is playing*/
        return AnimatorIsPlaying() && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void ChangeAnimationState(string newState) {
        newState += playerNumber ? "_p2" : "_p1";
        /*play an animation!*/
        if (currAnimState == newState) return;
        anim.Play(newState);
        currAnimState = newState;
    }

    //some utils

    void VisualBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance) {
        Vector2 p1, p2, p3, p4, p5, p6, p7, p8;
        float w = size.x * 0.5f;
        float h = size.y * 0.5f;
        p1 = new Vector2(-w, h);
        p2 = new Vector2(w, h);
        p3 = new Vector2(w, -h);
        p4 = new Vector2(-w, -h);

        Quaternion q = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
        p1 = q * p1;
        p2 = q * p2;
        p3 = q * p3;
        p4 = q * p4;

        p1 += origin;
        p2 += origin;
        p3 += origin;
        p4 += origin;

        Vector2 realDistance = direction.normalized * distance;
        p5 = p1 + realDistance;
        p6 = p2 + realDistance;
        p7 = p3 + realDistance;
        p8 = p4 + realDistance;


        //Drawing the cast
        //Color castColor = hit ? Color.red : Color.green;
        Color castColor = Color.red;
        Debug.DrawLine(p1, p2, castColor, 10);
        Debug.DrawLine(p2, p3, castColor, 10);
        Debug.DrawLine(p3, p4, castColor, 10);
        Debug.DrawLine(p4, p1, castColor, 10);

        Debug.DrawLine(p5, p6, castColor, 10);
        Debug.DrawLine(p6, p7, castColor, 10);
        Debug.DrawLine(p7, p8, castColor, 10);
        Debug.DrawLine(p8, p5, castColor, 10);

        Debug.DrawLine(p1, p5, Color.grey, 10);
        Debug.DrawLine(p2, p6, Color.grey, 10);
        Debug.DrawLine(p3, p7, Color.grey, 10);
        Debug.DrawLine(p4, p8, Color.grey, 10);
    }

}
