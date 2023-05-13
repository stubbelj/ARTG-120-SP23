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
    SpriteRenderer sr;
    Rigidbody2D rb;
    Dictionary<string, string> controlScheme;
    Dictionary<string, string>[] controlSchemeData = new Dictionary<string, string>[]{
        new Dictionary<string, string>{
            {"w", "up"},
            {"s", "down"},
            {"a", "left"},
            {"d", "right"},
            {"c", "attack"},
            {"v", "jump"},
            {"b", "block"}
        },
        new Dictionary<string, string>{
            {"up", "up"},
            {"down", "down"},
            {"left", "left"},
            {"right", "right"},
            {"[", "attack"},
            {"]", "jump"},
            {@"\", "block"}
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
                transform.Find("WalkColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                ChangeAnimationState("walk");
                if ((sr.flipX && dirInput == "left") || (!sr.flipX && dirInput == "right")) {
                    sr.flipX = !sr.flipX;
                    foreach(Transform colliderParent in transform) {
                        foreach(Transform frameParent in colliderParent) {
                            foreach(Transform box in frameParent) {
                                box.GetComponent<BoxCollider2D>().offset = new Vector2(-box.GetComponent<BoxCollider2D>().offset.x, box.GetComponent<BoxCollider2D>().offset.y);
                            }
                        }
                    }
                }
            } else if (dirInput == "down" && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                //crouch maybe? no animation right now, so just go into idle
                transform.Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            } else if (dirInput == "up" && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                //look up. no animation right now, so just go into idle
                transform.Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("idle");
            } else if (dirInput == null && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                transform.Find("IdleColliders").Find("Frame0").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
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
                    //no down attack right now
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
        currState = "block";
        stunned = true;
        ChangeAnimationState("block");
        transform.Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = true;
        transform.Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(1f);
        transform.Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        stunned = false;
        currState = null;
    }

    public void EndBlock() {
        //cancels your block, for when another player hits your block
        StopCoroutine(currActionCoroutine);
        currActionCoroutine = null;
        transform.Find("BlockColliders").gameObject.GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("BlockColliders").Find("Frame0").Find("BlockHurtbox").gameObject.GetComponent<BoxCollider2D>().enabled = false;
        stunned = false;
        currState = null;
    }

    void DisableHitHurtBoxes() {
        /*disables all hit and hurt boxes, effectively making the player invincible and unable to deal damage. used by block
        because it does NOT DISABLE BLOCK HITBOX*/
        foreach(Transform colliderParent in transform) {
            foreach(Transform frameParent in colliderParent) {
                foreach(Transform box in frameParent) {
                    if (box.tag == "Hitbox" || box.tag == "Hurtbox") {
                        box.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }
    }

    //All of these just toggle hitboxes for the frames of an attack
    IEnumerator SideAttack() {
        currState = "attacking";
        yield return new WaitForSeconds(0.15f);

        Transform frame1 = transform.Find("SideAttackColliders").Find("Frame1");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("side_attack");
        yield return new WaitForSeconds(0.15f);

        Transform frame2 = transform.Find("SideAttackColliders").Find("Frame2");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.15f);

        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator UpAttack() {
        currState = "attacking";
        yield return new WaitForSeconds(0.15f);

        Transform frame1 = transform.Find("UpAttackColliders").Find("Frame1");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("up_attack");
        yield return new WaitForSeconds(0.15f);

        Transform frame2 = transform.Find("UpAttackColliders").Find("Frame2");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.15f);

        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator UpAirAttack() {
        currState = "attacking";
        yield return new WaitForSeconds(0.15f);

        Transform frame1 = transform.Find("UpAirAttackColliders").Find("Frame1");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("up_air_attack");
        yield return new WaitForSeconds(0.15f);

        Transform frame2 = transform.Find("UpAirAttackColliders").Find("Frame2");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.15f);

        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
        currActionCoroutine = null;
    }

    IEnumerator DownAirAttack() {
        currState = "attacking";
        yield return new WaitForSeconds(0.15f);

        Transform frame1 = transform.Find("DownAirAttackColliders").Find("Frame1");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("down_air_attack");
        yield return new WaitForSeconds(0.15f);

        Transform frame2 = transform.Find("DownAirAttackColliders").Find("Frame2");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.15f);

        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        currState = null;
        currActionCoroutine = null;
    }

    public void TakeDamage(AttackData aD, Vector3 contactPoint) {
        //called by hitboxes when they contact a hurtbox, adds invlun to attack instances and calls HitStunAndLaunch
        if ((!damagedBy.Contains(aD.damageInst)) && (aD.multiHit || !attackedBy.Contains(aD.attackId))) {
            //Vector3 forceVec = ((Vector3)transform.position - contactPoint).normalized;
            Vector3 forceVec = ((Vector3)transform.position - gameManager.players[playerNumber ? 0 : 1].transform.position).normalized;
            percent += aD.damage;
            percentText.text = percent.ToString();
            forceVec *= Mathf.Pow(aD.damage * percent, 1.3f);
            StartCoroutine(HitStunAndLaunch(0.1f, aD, forceVec));
        }
        damagedBy.Add(aD.damageInst);
        attackedBy.Add(aD.attackId);
        StartCoroutine(RefreshInvulnById(aD.damageInst, aD.attackId));
    }

    public IEnumerator HitStunAndLaunch(float mag, AttackData ad, Vector3 forceVec) {
        //called when you take damage, makes you get stunned, shake a lil and then get launched
        Vector3 originPos = transform.position;
        //forcefully change player state to be stunned
        stunned = true;
        currState = "hurt";
        ChangeAnimationState("hurt");
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

    IEnumerator RefreshInvulnById(int damageInst, int attackId) {
        //makes it so that you're invulnerable to a specific instnace of an attack (ex. THAT ONE TIME the player pressed attack) for a lil bit
        yield return new WaitForSeconds(0.1f);
        damagedBy.Remove(damageInst);
        attackedBy.Remove(attackId);
    }

    void LoadAttackData() {
        //initializes hitboxes with data about their respective attacks
        Transform sideAttack = transform.Find("SideAttackColliders");
        int sideAttackId = AttackData.attackIdFlow++;
        sideAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(10f, 0.1f, true, this, sideAttackId);
        sideAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.1f, true, this, sideAttackId);

        Transform upAttack = transform.Find("UpAttackColliders");
        int upAttackId = AttackData.attackIdFlow++;
        upAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(10f, 0.1f, true, this, upAttackId);
        upAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.1f, true, this, upAttackId);

        Transform upAirAttack = transform.Find("UpAirAttackColliders");
        int upAirAttackId = AttackData.attackIdFlow++;
        upAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(10f, 0.1f, true, this, upAirAttackId);
        upAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.1f, true, this, upAirAttackId);

        Transform downAirAttack = transform.Find("DownAirAttackColliders");
        int downAirAttackId = AttackData.attackIdFlow++;
        downAirAttack.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(10f, 0.1f, true, this, downAirAttackId);
        downAirAttack.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(3f, 0.1f, true, this, downAirAttackId);
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
        /*check if a specific animation is playing*/
        return AnimatorIsPlaying() && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void ChangeAnimationState(string newState) {
        /*play an animation!*/
        //if (currAnimState == newState) return;
        //felt really smart writing this until i realized that it's not safe across threads
        anim.Play(newState);
        currAnimState = newState;
    }

}
