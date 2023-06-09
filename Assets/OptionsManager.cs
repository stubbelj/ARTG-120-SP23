using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public GameObject mainWindow;
    public GameObject controlsWindow;
    public GameObject videoWindow;
    public GameObject audioWindow;

    public GameObject backButton;

    public GameObject closeBG;
    public GameObject farBG;

    public Sprite[] controlLabels;
    public Sprite[] controlGuides;
    //keyboard1, keyboard2, controller1, controller2
    public GameObject p1ControlLabel;
    public GameObject p2ControlLabel;
    public GameObject p1ControlGuide;
    public GameObject p2ControlGuide;

    GameManager gameManager;

    public float contrast = 0f;
    public float musicVolumeMod = 0f;
    public float soundeffectsVolumeMod = 0f;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.inst;
    }
    
    public void P1ToggleControlsLeft() {
        print("left");
        gameManager.p1Scheme--;
        if (gameManager.p1Scheme < 0) {
            gameManager.p1Scheme = 3;
        }
        p1ControlGuide.GetComponent<SpriteRenderer>().sprite = controlGuides[gameManager.p1Scheme];
        p1ControlLabel.GetComponent<SpriteRenderer>().sprite = controlLabels[gameManager.p1Scheme];
        PlayerPrefs.SetInt("p1Scheme", gameManager.p1Scheme);
    }
    
    public void P1ToggleControlsRight() {
        gameManager.p1Scheme++;
        if (gameManager.p1Scheme > 3) {
            gameManager.p1Scheme = 0;
        }
        p1ControlGuide.GetComponent<SpriteRenderer>().sprite = controlGuides[gameManager.p1Scheme];
        p1ControlLabel.GetComponent<SpriteRenderer>().sprite = controlLabels[gameManager.p1Scheme];
        PlayerPrefs.SetInt("p1Scheme", gameManager.p1Scheme);
    }

    public void P2ToggleControlsLeft() {
        gameManager.p2Scheme--;
        if (gameManager.p2Scheme < 0) {
            gameManager.p2Scheme = 3;
        }
        p2ControlGuide.GetComponent<SpriteRenderer>().sprite = controlGuides[gameManager.p2Scheme];
        p2ControlLabel.GetComponent<SpriteRenderer>().sprite = controlLabels[gameManager.p2Scheme];
        PlayerPrefs.SetInt("p2Scheme", gameManager.p2Scheme);
    }
    
    public void P2ToggleControlsRight() {
        gameManager.p2Scheme++;
        if (gameManager.p2Scheme > 3) {
            gameManager.p2Scheme = 0;
        }
        p2ControlGuide.GetComponent<SpriteRenderer>().sprite = controlGuides[gameManager.p2Scheme];
        p2ControlLabel.GetComponent<SpriteRenderer>().sprite = controlLabels[gameManager.p2Scheme];
        PlayerPrefs.SetInt("p2Scheme", gameManager.p2Scheme);
    }

    public void BrightnessToggleLeft() {
        if (Screen.brightness > 0) {
            Screen.brightness -= 0.05f;
        }
        PlayerPrefs.SetFloat("brightness", Screen.brightness);
    }

    public void BrightnessToggleRight() {
        if (Screen.brightness < 1.0f) {
            Screen.brightness += 0.05f;
        }
        PlayerPrefs.SetFloat("brightness", Screen.brightness);
    }

    public void ContrastToggleLeft() {
        if (contrast > 0) {
            contrast -= 0.05f;
            Color origin = new Color(1, 1, 1);
            closeBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - contrast, origin.g - contrast, origin.b - contrast, 1);
            farBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - contrast, origin.g - contrast, origin.b - contrast, 1);
            PlayerPrefs.SetFloat("contrast", contrast);
            print(contrast);
        }
    }

    public void ContrastToggleRight() {
        if (contrast < 1.0f) {
            contrast += 0.05f;
            Color origin = new Color(1, 1, 1);
            closeBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - contrast, origin.g - contrast, origin.b - contrast, 1);
            farBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - contrast, origin.g - contrast, origin.b - contrast, 1);
            PlayerPrefs.SetFloat("contrast", contrast);
            print(contrast);
        }
    }

    public void MusicVolumeModToggleLeft() {
        if (musicVolumeMod > 0) {
            musicVolumeMod -= 0.05f;
            gameManager.musicVolumeMod = musicVolumeMod;
        }
    }

    public void MusicVolumeModToggleRight() {
        if (musicVolumeMod < 1.0f) {
            musicVolumeMod += 0.05f;
            gameManager.musicVolumeMod = musicVolumeMod;
        }
    }

    public void SoundeffectsVolumeModToggleLeft() {
        if (soundeffectsVolumeMod > 0) {
            soundeffectsVolumeMod -= 0.05f;
            gameManager.soundeffectsVolumeMod = soundeffectsVolumeMod;
        }
    }

    public void SoundeffectsVolumeModToggleRight() {
        if (soundeffectsVolumeMod < 1.0f) {
            soundeffectsVolumeMod += 0.05f;
            gameManager.soundeffectsVolumeMod = soundeffectsVolumeMod;
        }
    }

    public void GoBack() {
        controlsWindow.SetActive(false);
        videoWindow.SetActive(false);
        audioWindow.SetActive(false);
        mainWindow.SetActive(true);
        backButton.SetActive(false);
    }

    public void OpenWindow(string windowName) {
        mainWindow.SetActive(false);
        if (windowName == "controls") {
            controlsWindow.SetActive(true);
            videoWindow.SetActive(false);
            audioWindow.SetActive(false);
            backButton.SetActive(true);
        } else if (windowName == "video") {
            controlsWindow.SetActive(false);
            videoWindow.SetActive(true);
            audioWindow.SetActive(false);
            backButton.SetActive(true);
        } else if (windowName == "audio") {
            controlsWindow.SetActive(false);
            videoWindow.SetActive(false);
            audioWindow.SetActive(true);
            backButton.SetActive(true);
        }
    }
}
