using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    GameManager gameManager;
    float moveSpeed = 0.3f;
    bool playerNumber;
    string currAction = "idle";
    //should never be null
    public Animator anim;
    //this is an animator component on this object, which runs animation
    string currAnimState;
    //current animation
    SpriteRenderer sr;
    Rigidbody2D rb;
    Dictionary<string, string> controlScheme;
    Dictionary<string, string>[] controlSchemeData = new Dictionary<string, string>[]{
        new Dictionary<string, string>{
            {"w", "up"},
            {"s", "down"},
            {"a", "left"},
            {"d", "right"},
            {"g", "attack"},
            {"h", "block"}
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

        //save input from this frame
        inputs[1] = inputs[0];
    }

    void ReduceInputs() {
        /*reduce conflicting inputs to valid inputs*/

        //left, right, up and down are non-simultaneous holds
        //all other actions are non-simultaneous presses

        //reduce directional inputs down to a single directional input
        if (((List<bool>)inputs[0].Values.ToList().GetRange(0, 4)).Count(x => x) > 1) {
            //colliding dir inputs exist
            foreach(KeyValuePair<string, bool> pair in inputs[0]) {
                if (pair.Value && inputs[1][pair.Key]) {
                    //overlapping old inputs, erase from new inputs
                    inputs[0][pair.Key] = false;
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

        string dirInput;
        foreach (KeyValuePair<string, bool> pair in inputs[0]) {
            if (pair.Value) { dirInput = pair.Key;}
        }
        //value that's actually being used in this implementation

        //reduce action inputs down to a single action input
        if (((List<bool>)inputs[0].Values.ToList().GetRange(0, 4)).Count(x => x) > 1) {
            //colliding action inputs exist
            foreach(KeyValuePair<string, bool> pair in inputs[0]) {
                if (pair.Value && inputs[1][pair.Key]) {
                    //overlapping old inputs, erase from new inputs
                    inputs[0][pair.Key] = false;
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

        string actionInput;
        foreach (KeyValuePair<string, bool> pair in inputs[0]) {
            if (pair.Value) { actionInput = pair.Key;}
        }
        //value that's actually being used in this implementation
    }

    void ExecuteInputs() {
        /*have the player perform actions based on the (reduced) input*/
        //
    }

    public void Init(bool newPlayerNumber, string newControlScheme) {
        playerNumber = newPlayerNumber;
        controlScheme = controlSchemeData[1];
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
