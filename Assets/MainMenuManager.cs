using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{    
    public static MainMenuManager inst = null;
    public OptionsManager optionsManager;
    public AudioClip audioClip;

    float musicVolumeMod;
    public float soundeffectsVolumeMod;
    int p1Scheme;
    int p2Scheme;

    public GameObject closeBG;
    public GameObject farBG;
    public GameObject optionsWindow;
    public GameObject creditsWindow;

    void Awake() {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {

        if (PlayerPrefs.HasKey("p1Scheme")) {
            p1Scheme = PlayerPrefs.GetInt("p1Scheme");
            p2Scheme = PlayerPrefs.GetInt("p2Scheme");
        } else {
            p1Scheme = 0;
            p2Scheme = 1;
            PlayerPrefs.SetInt("p1Scheme", 0);
            PlayerPrefs.SetInt("p2Scheme", 1);
        }

        //debugText = GameObject.Find("debugText").GetComponent<TMP_Text>();

        if (PlayerPrefs.HasKey("brightness")) {
            Screen.brightness = PlayerPrefs.GetFloat("brightness");
        } else {
            Screen.brightness = 0.5f;
            PlayerPrefs.SetFloat("brightness", 0.5f);
        }

        float tempContrast;
        if (PlayerPrefs.HasKey("contrast")) {
            tempContrast = PlayerPrefs.GetFloat("contrast");
        } else {
            tempContrast = 0f;
            PlayerPrefs.SetFloat("contrast", 0f);
        }

        if (PlayerPrefs.HasKey("musicVolumeMod")) {
            musicVolumeMod = PlayerPrefs.GetFloat("musicVolumeMod");
        } else {
            musicVolumeMod = 0.5f;
            PlayerPrefs.SetFloat("musicVolumeMod", musicVolumeMod);
        }
        optionsManager.musicVolumeMod = musicVolumeMod;

        if (PlayerPrefs.HasKey("soundeffectsVolumeMod")) {
            soundeffectsVolumeMod = PlayerPrefs.GetFloat("soundeffectsVolumeMod");
        } else {
            soundeffectsVolumeMod = 0.5f;
            PlayerPrefs.SetFloat("soundeffectsVolumeMod", soundeffectsVolumeMod);
        }
        optionsManager.soundeffectsVolumeMod = soundeffectsVolumeMod;

        Color origin = new Color(1, 1, 1, 1);
        closeBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - tempContrast, origin.g - tempContrast, origin.b - tempContrast, 1);
        farBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - tempContrast, origin.g - tempContrast, origin.b - tempContrast, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape")) {
            print("pressed key escape");
            ToggleCredits();
        }
    }
    
    public void ToggleOptions() {
        GetComponent<AudioSource>().PlayOneShot(audioClip, soundeffectsVolumeMod);
        if (Time.timeScale == 1) {
            Time.timeScale = 0;
            optionsWindow.SetActive(true);
        } else {
            Time.timeScale = 1;
            optionsWindow.SetActive(false);
        }
    }

    bool creditsToggled = false;
    public void ToggleCredits() {
        GetComponent<AudioSource>().PlayOneShot(audioClip, soundeffectsVolumeMod);
        if (!creditsToggled) {
            creditsToggled = true;
            creditsWindow.SetActive(true);
        } else {
            creditsToggled = false;
            creditsWindow.SetActive(false);
        }
    }
}
