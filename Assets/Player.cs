using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Player : MonoBehaviour
{
    Vector2 moveSpeed = new Vector2(100, 100);
    Vector2 maxSpeed = new Vector2(15f, 15f);
    float jumpForce = 1100f;
    bool isGrounded = false;
    int jumps = 0;
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
    Dictionary<string, string>[] controlSchemeData = new Dictionary<string, string>[]{
        new Dictionary<string, string>{
            {"w", "up"},
            {"s", "down"},
            {"a", "left"},
            {"d", "right"},
            {"z", "attack"},
            {"x", "jump"},
            {"c", "block"},
            {"v", "grab"}
        },
        new Dictionary<string, string>{
            {"up", "up"},
            {"down", "down"},
            {"left", "left"},
            {"right", "right"},
            {"u", "attack"},
            {"i", "jump"},
            {"o", "block"},
            {"p", "grab"}
        }
    };

    Dictionary<string, bool>[] inputs = new Dictionary<string, bool>[]{
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
    
    void Awake()
    {
        gameManager = GameManager.inst;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        LoadAttackData();
    }
    
    void Update()
    {

        //store keys pressed this frame in inputs[0]
        foreach (string key in controlScheme.Keys) {
            if(Input.GetKey(key)) {
                inputs[0][controlScheme[key]] = true;
            } else {
                inputs[0][controlScheme[key]] = false;
            }
        }

        ReduceInputs();

        ExecuteInputs();

        //rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed.x, maxSpeed.x), Mathf.Clamp(rb.velocity.y, -maxSpeed.y, maxSpeed.y), 0);

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
        List<bool> dirVals = new List<bool>(inputs[0].Values.ToList().GetRange(0, 4));
        List<string> actionKeys = new List<string>(inputs[0].Keys.ToList().GetRange(4, inputs[0].Count - 4));
        List<bool> actionVals = new List<bool>(inputs[0].Values.ToList().GetRange(4, inputs[0].Count - 4));
        //reduce directional inputs down to a single directional input
        if (((List<bool>)inputs[0].Values.ToList().GetRange(0, 4)).Count(x => x) > 1) {
            //colliding dir inputs exist
            for (int i = 0; i < 4; i++) {
                if (dirVals[i] && inputs[1][dirKeys[i]]) {
                    //overlapping old inputs, erase from new inputs
                    inputs[0][dirKeys[i]] = false;
                }
            }
            //keep one of the dir inputs - they might still be colliding
            /*probably doesn't matter if some inputs hold precedence, but i selected the input to keep randomly anyway
            List<string> inputsToRemove = new List<string>{"up", "down", "left", "right"};
            inputsToRemove.RemoveAt(gameManager.rand.Next(0, 4));
            foreach (string inputName in inputsToRemove) {
                inputs[0][inputName] = false;
            }*/
            List<string> inputsToRemove = new List<string>{"up", "down", "left", "right"};
            if (inputs[0]["up"]) {
                inputs[0]["down"] = inputs[0]["left"] = inputs[0]["right"] = false;
            }
            else if (inputs[0]["down"]) {
                inputs[0]["up"] = inputs[0]["left"] = inputs[0]["right"] = false;
            }
            else if (inputs[0]["left"]) {
                inputs[0]["down"] = inputs[0]["up"] = inputs[0]["right"] = false;
            } else {
                inputs[0]["down"] = inputs[0]["left"] = inputs[0]["up"] = false;
            }
        }

        for (int i = 0; i < 4; i++) {
            if (dirVals[i]) {dirInput = dirKeys[i];}
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
            List<string> inputsToRemove = new List<string>{"attack", "block"};
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
            if ((dirInput == "left" || dirInput == "right") && (new string[]{null, "idle", "walking"}.Contains(currState))) {
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
            } else if (dirInput == "down" && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                //crouch maybe? no animation right now, so just go into idle
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            } else if (dirInput == "up" && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                //look up. no animation right now, so just go into idle
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            } else if (dirInput == null && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                transform.Find("Colliders").Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            }

        } else if (actionInput == "attack" && new string[]{null, "idle", "walking"}.Contains(currState)) {
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
                    currActionCoroutine = StartCoroutine(UpAirAttack());
                } else if (dirInput == "down") {
                    currActionCoroutine = StartCoroutine(DownAirAttack());
                } else if (dirInput == null) {
                    currActionCoroutine = StartCoroutine(SideAttack());
                }
            }
        } else if (actionInput == "jump") {
            if (jumps > 0) {
                currActionCoroutine = StartCoroutine(ForceOverTime(new Vector3(0, jumpForce, 0), 0.1f));
                isGrounded = false;
                jumps--;
            }
        } else if (actionInput == "block" && new string[]{null, "idle", "walking"}.Contains(currState)) {
            currActionCoroutine = StartCoroutine(Block());
        } else if (actionInput == "grab" && new string[]{null, "idle", "walking"}.Contains(currState)) {
            currActionCoroutine = StartCoroutine(Grab());
        }
    }

    public void OnTriggerEnter2D(Collider2D col) {
        //just for platforming and death box
        if (col.gameObject.tag == "Stage" && isGrounded == false) {
            isGrounded = true;
            jumps = 1;
        } else if (col.gameObject.tag == "DeathBox") {
            Die();
        }
    }

    IEnumerator Block() {
        //enables block collider and hides all other colliders 
        DisableHitHurtBoxes();
        currState = "blocking";
        stunned = true;
        ChangeAnimationState("block");
        transform.Find("Colliders").Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        transform.Find("Colliders").Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(1f);
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
        yield return new WaitForSeconds(0.3f);
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
        yield return new WaitForSeconds(0.05f);
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
        AttackData grabAttackData = new AttackData(7, 0.0f, false, gameManager.players[playerNumber ? 1 : 0], AttackData.attackIdFlow++);
        TakeDamage(grabAttackData, (transform.position - gameManager.players[playerNumber ? 0 : 1].transform.position) / 2);
    }

    //All of these just toggle hitboxes for the frames of an attack
    IEnumerator SideAttack() {
        currState = "attacking";
        ChangeAnimationState("sideSlash");
        yield return new WaitForSeconds(0.3f / 8);
        
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
            yield return new WaitForSeconds(0.3f / 8);
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
        currState = "attacking";
        ChangeAnimationState("downSlash");
        yield return new WaitForSeconds(0.3f / 8);
        
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
            yield return new WaitForSeconds(0.3f / 8);
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
            yield return new WaitForSeconds(0.3f / 8);
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
        currState = "attacking";
        ChangeAnimationState("up_air_attack");
        yield return new WaitForSeconds(0.3f / 8);

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
            yield return new WaitForSeconds(0.3f / 8);
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
        currState = "attacking";
        ChangeAnimationState("down_air_attack");
        yield return new WaitForSeconds(0.3f / 8);

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
            yield return new WaitForSeconds(0.3f / 8);
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
            forceVec *= aD.damage * percent * 75f;
            StartCoroutine(HitStunAndLaunch(0.1f, aD, forceVec));
        }
        damagedBy.Add(aD.damageInst);
        attackedBy.Add(aD.attackId);
        StartCoroutine(RefreshInvulnById(aD.damageInst, aD.attackId));
    }

    public IEnumerator HitStunAndLaunch(float mag, AttackData ad, Vector3 forceVec) {
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
        rb.AddForce(forceVec);
        stunned = false;
        currState = null;
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
        sideAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, sideAttackId);
        sideAttack.Find("Frame5").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, this, sideAttackId);

        Transform upAttack = transform.Find("Colliders").Find("UpAttackColliders");
        int upAttackId = AttackData.attackIdFlow++;
        upAttack.Find("Frame3").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, upAttackId);
        upAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, upAttackId);
        upAttack.Find("Frame5").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, this, upAttackId);

        Transform downAttack = transform.Find("Colliders").Find("DownAttackColliders");
        int downAttackId = AttackData.attackIdFlow++;
        downAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, downAttackId);
        downAttack.Find("Frame3").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, downAttackId);
        downAttack.Find("Frame4").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, this, downAttackId);

        Transform upAirAttack = transform.Find("Colliders").Find("UpAirAttackColliders");
        int upAirAttackId = AttackData.attackIdFlow++;
        upAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, upAirAttackId);
        upAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, this, upAirAttackId);

        Transform downAirAttack = transform.Find("Colliders").Find("DownAirAttackColliders");
        int downAirAttackId = AttackData.attackIdFlow++;
        downAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, 0.15f, true, this, downAirAttackId);
        downAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.15f, true, this, downAirAttackId);
    }

    IEnumerator ForceOverTime(Vector3 force, float time) {
        //applies a force over time, used for making the jump feel right
        while(time > 0) {
            rb.AddForce(force * (0.1f / time));
            yield return new WaitForSeconds(0.1f);
            time -= 0.1f;
        }
    }

    public void Init(bool newPlayerNumber, int newControlScheme, TMP_Text newPercentText) {
        //initializer bc monobehaviours don't really get constructors and Start() isn't safe
        playerNumber = newPlayerNumber;
        controlScheme = controlSchemeData[newControlScheme];
        percentText = newPercentText;
    }

    void Die() {
        //destroy self and ask gameManager to reset the game
        gameManager.AwardPoint(!playerNumber);
        Destroy(gameObject);
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
        //felt really smart writing this until i realized that it's not safe across threads
        anim.Play(newState);
        currAnimState = newState;
    }

}
