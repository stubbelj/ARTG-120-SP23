using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicToggleButton : MonoBehaviour
{
    public string buttonType;
    
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.inst;
    }

    void OnMouseDown(){
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], gameManager.soundeffectsVolumeMod);
        if (buttonType == "musicL") {
            gameManager.optionsManager.MusicVolumeModToggleLeft();
        } else if (buttonType == "musicR") {
            gameManager.optionsManager.MusicVolumeModToggleRight();
        } else if (buttonType == "soundeffectsL") {
            gameManager.optionsManager.SoundeffectsVolumeModToggleLeft();
        } else if (buttonType == "soundeffectsR") {
            gameManager.optionsManager.SoundeffectsVolumeModToggleRight();
        }
    }
}
