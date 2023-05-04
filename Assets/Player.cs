using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    Vector2 moveSpeed = new Vector2(100, 100);
    Vector2 maxSpeed = new Vector2(15f, 15f);
    float jumpForce = 1100f;
    bool isGrounded = false;
    int jumps = 0;
    bool playerNumber;
    float percent = 0;

    string actionInput = null;
    string dirInput = null;
    public Animator anim;
    //this is an animator component on this object, which runs animation
    string currAnimState;
    //current animation
    string currState = null;

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
            {"h", "block"}
        },
        new Dictionary<string, string>{
            {"up", "up"},
            {"down", "down"},
            {"left", "left"},
            {"right", "right"},
            {"[", "attack"},
            {"]", "jump"},
            {"p", "block"}
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
            //probably doesn't matter if some inputs hold precedence, but i selected the input to keep randomly anyway
            List<string> inputsToRemove = new List<string>{"up", "down", "left", "right"};
            inputsToRemove.RemoveAt(gameManager.rand.Next(0, 4));
            foreach (string inputName in inputsToRemove) {
                inputs[0][inputName] = false;
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
                }
                rb.velocity += (Mathf.Abs(rb.velocity.x) < maxSpeed.x ? 1 : 0) * new Vector2((isGrounded ? 1 : 0.5f) * (dirInput == "right" ? 1 : -1) * moveSpeed.x * turnAroundFactor * Time.deltaTime, 0);
                transform.Find("WalkColliders").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                ChangeAnimationState("test_walk");
                if ((sr.flipX && dirInput == "left") || (!sr.flipX && dirInput == "right")) {
                    sr.flipX = !sr.flipX;
                    foreach(Transform frameObj in transform.Find("StabColliders")) {
                        foreach(Transform box in frameObj) {
                            box.localPosition = new Vector3(-box.localPosition.x, box.localPosition.y, 0);
                        }
                    }
                }
            } else if (dirInput == "down") {
                rb.velocity += new Vector2(0, (isGrounded ? 0 : -moveSpeed.y));
                //crouch maybe?
            } else if (dirInput == "up") {
                //upwards attack
            } else if (dirInput == null && (new string[]{null, "idle", "walking"}.Contains(currState))) {
                transform.Find("IdleColliders").Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
                currState = "idle";
                ChangeAnimationState("test_idle");
            }

        } else if (actionInput == "attack" && (new string[]{null, "idle", "walking"}.Contains(currState))) {
            if (dirInput == "left" || dirInput == "right") {
                StartCoroutine(StabAttack());
            } else if (dirInput == "up") {
                //upwards attack
            } else if (dirInput == "down") {
                //downwards attack
            } else if (dirInput == null) {
                StartCoroutine(StabAttack());
            }
        } else if (actionInput == "jump") {
            if (jumps > 0) {
                StartCoroutine(ForceOverTime(new Vector3(0, jumpForce, 0), 0.1f));
                isGrounded = false;
                jumps--;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Stage" && isGrounded == false) {
            isGrounded = true;
            jumps = 1;
        } else if (col.gameObject.tag == "DeathBox") {
            Die();
        }
    }

    IEnumerator StabAttack() {
        currState = "attacking";
        Transform frame1 = transform.Find("StabColliders").Find("Frame1");

        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        ChangeAnimationState("test_stab");
        yield return new WaitForSeconds(0.3f);

        Transform frame2 = transform.Find("StabColliders").Find("Frame2");
        frame1.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame1.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = true;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.5f);

        frame2.Find("Hitbox").GetComponent<BoxCollider2D>().enabled = false;
        frame2.Find("Hurtbox").GetComponent<BoxCollider2D>().enabled = false;

        currState = null;
    }

    public void TakeDamage(AttackData aD, Vector3 contactPoint) {
        if ((!damagedBy.Contains(aD.damageInst)) && (aD.multiHit || !attackedBy.Contains(aD.attackId))) {
            Vector3 forceVec = ((Vector3)transform.position - contactPoint).normalized;
            percent += aD.damage;
            forceVec *= aD.damage * percent * 10;
            rb.AddForce(forceVec);
        }
        damagedBy.Add(aD.damageInst);
        attackedBy.Add(aD.attackId);
        StartCoroutine(RefreshInvlunById(aD.damageInst, aD.attackId));
    }

    IEnumerator RefreshInvlunById(int damageInst, int attackId) {
        yield return new WaitForSeconds(0.1f);
        damagedBy.Remove(damageInst);
        attackedBy.Remove(attackId);
    }

    void LoadAttackData() {
        Transform stab = transform.Find("StabColliders");
        int stabId = AttackData.attackIdFlow++;
        stab.Find("Frame1").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(1f, true, this, stabId);
        stab.Find("Frame2").Find("Hitbox").GetComponent<Hitbox>().attackData = new AttackData(10f, true, this, stabId);
    }

    IEnumerator ForceOverTime(Vector3 force, float time) {
        print("jumpin");
        while(time > 0) {
            rb.AddForce(force * (0.1f / time));
            yield return new WaitForSeconds(0.1f);
            time -= 0.1f;
        }
    }

    public void Init(bool newPlayerNumber, int newControlScheme) {
        playerNumber = newPlayerNumber;
        controlScheme = controlSchemeData[newControlScheme];
    }

    void Die() {
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
        //checks if an anim is even playing, then gets the name of the anim and checks if it is the same as the anim controller's stateName
    }

    public void ChangeAnimationState(string newState) {
        /*play an animation!*/
        if (currAnimState == newState) return;
        //if the current animation is already playing, don't interrupt it and start the animation over
        //if you want to cleanly loop an animation, you can set it as a loopable animation in the animation controller
        anim.Play(newState);
        currAnimState = newState;
    }

}
