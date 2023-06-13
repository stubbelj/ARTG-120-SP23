using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuButton : MonoBehaviour
{
    public string buttonType;
    
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.inst;
    }

    void OnMouseDown(){
        //gameManager.audioSource.PlayOneShot(gameManager.audioClips[3], gameManager.soundeffectsVolumeMod);
        if (buttonType == "controls") {
            gameManager.optionsManager.OpenWindow("controls");
        } else if (buttonType == "video") {
            gameManager.optionsManager.OpenWindow("video");
        } else if (buttonType == "audio") {
            gameManager.optionsManager.OpenWindow("audio");
        } else if (buttonType == "restart") {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Time.timeScale = 1;
        } else if (buttonType == "exit") {
            Application.Quit();
        }
    }
}
