using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour
{
    
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.inst;
    }

    void OnMouseDown(){
        gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], gameManager.soundeffectsVolumeMod);
        gameManager.optionsManager.GoBack();
    }
}
