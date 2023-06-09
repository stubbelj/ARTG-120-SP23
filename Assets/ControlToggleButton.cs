using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlToggleButton : MonoBehaviour
{
    public string buttonType;
    
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.inst;
    }

    void OnMouseDown(){
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], gameManager.soundeffectsVolumeMod);
        if (buttonType == "p1L") {
            gameManager.optionsManager.P1ToggleControlsLeft();
        } else if (buttonType == "p1R") {
            gameManager.optionsManager.P1ToggleControlsRight();
        } else if (buttonType == "p2L") {
            gameManager.optionsManager.P2ToggleControlsLeft();
        } else if (buttonType == "p2R") {
            gameManager.optionsManager.P2ToggleControlsRight();
        }
    }
}
