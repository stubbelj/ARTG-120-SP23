using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class titleButton : MonoBehaviour
{
    public string buttonType;
    MainMenuManager mainMenuManager;

    void Start()
    {
        mainMenuManager = MainMenuManager.inst;
    }

    void OnMouseDown(){
        //mainMenuManager.GetComponent<AudioSource>().PlayOneShot(mainMenuManager.audioClip, mainMenuManager.soundeffectsVolumeMod);
        if (buttonType == "start") {
            SceneManager.LoadScene(1);
        } else if (buttonType == "credits") {
            mainMenuManager.ToggleCredits();
        } else if (buttonType == "escape") {
            mainMenuManager.ToggleCredits();
        }
    }
}
