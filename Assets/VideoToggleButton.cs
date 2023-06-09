using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoToggleButton : MonoBehaviour
{
    public string buttonType;
    
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.inst;
    }

    void OnMouseDown(){
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], gameManager.soundeffectsVolumeMod);
        if (buttonType == "brightnessL") {
            gameManager.optionsManager.BrightnessToggleLeft();
        } else if (buttonType == "brightnessR") {
            gameManager.optionsManager.BrightnessToggleRight();
        } else if (buttonType == "contrastL") {
            gameManager.optionsManager.ContrastToggleLeft();
        } else if (buttonType == "contrastR") {
            gameManager.optionsManager.ContrastToggleRight();
        }
    }
}
