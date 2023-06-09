using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public int p1Scheme;
    public int p2Scheme;
    //0 is wasd + vbnm
    //1 is arrowkeys + uiop
    //2 is controller 1, works for xbox and playstation


    public static GameManager inst = null;
    public System.Random rand = new System.Random();
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints = new Transform[2];
    public GameObject[] scorePoints = new GameObject[2];
    public Sprite[] scoreSprites = new Sprite[3];
    public int[] scores = new int[]{0, 0};
    public GameObject[] percentTexts;
    public Player[] players = new Player[2];
    public bool[] usesController = new bool[2];
    public List<string>[] playerPerks = {new List<string>(), new List<string>()};
    public GameObject optionsWindow;
    GameObject escapeButton;
    string[] perkOptionsNames = new string[3];
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    public OptionsManager optionsManager;
    public GameObject closeBG;
    public GameObject farBG;
    public float musicVolumeMod;
    public float soundeffectsVolumeMod;
    public TMP_Text debugText;
    public Sprite[] p1ScoreDisplays = new Sprite[3];
    public Sprite[] p2ScoreDisplays = new Sprite[3];
    public GameObject p1ScoreDisplay;
    public GameObject p2ScoreDisplay;
    public Sprite[] p1FireWorks = new Sprite[4];
    public Sprite[] p2FireWorks = new Sprite[4];
    public GameObject fireWorkPrefab;
    bool gameWon = false;
    public GameObject p1WinText;
    public GameObject p2WinText;

    List<string> perkPool = new List<string>{
        "superSpeed", "superSpeed", "superSpeed"
    };

    public int Getp1Scheme() {
        return p1Scheme;
    }

    public int Getp2Scheme(){
        return p2Scheme;
    }

    bool awardingPoint = false;
    // Start is called before the first frame update
    void Awake() {


        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }

        p1Scheme = 0;
        p2Scheme = 1;

        players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
        players[0].Init(false, p1Scheme, percentTexts[0].GetComponent<TMP_Text>(), p1Scheme == 2 ? true : false);
        Player.activeControlSchemes[0] = p1Scheme;
        usesController[0] = p1Scheme == 2 ? true : false;

        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[1].Init(true, p2Scheme, percentTexts[1].GetComponent<TMP_Text>(), p2Scheme == 2 ? true : false);
        Player.activeControlSchemes[1] = p2Scheme;
        usesController[1] = p2Scheme == 2 ? true : false;

        GameObject.Find("BGMusicSource").GetComponent<AudioSource>().Play();
        escapeButton = GameObject.Find("EscapeButton");

        if (PlayerPrefs.HasKey("p1Scheme")) {
            p1Scheme = PlayerPrefs.GetInt("p1Scheme");
            p2Scheme = PlayerPrefs.GetInt("p2Scheme");
        } else {
            p1Scheme = 0;
            p2Scheme = 1;
            PlayerPrefs.SetInt("p1Scheme", 0);
            PlayerPrefs.SetInt("p2Scheme", 1);
        }
        Player.activeControlSchemes[0] = p1Scheme;
        usesController[0] = p1Scheme == 2 || p1Scheme == 3 ? true : false;
        players[0].controlSchemeController = p1Scheme == 2 || p1Scheme == 3 ? true : false;
        players[0].controlScheme = Player.controlSchemeData[p1Scheme];
        Player.activeControlSchemes[1] = p2Scheme;
        usesController[1] = p2Scheme == 2 || p1Scheme == 3 ? true : false;
        players[1].controlScheme = Player.controlSchemeData[p2Scheme];
        players[1].controlSchemeController = p1Scheme == 2 || p1Scheme == 3 ? true : false;

        optionsManager.controlsWindow.transform.Find("P1Controls").Find("ControlTitle").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlLabels[p1Scheme];
        optionsManager.controlsWindow.transform.Find("P1Controls").Find("ControlGuide").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlGuides[p1Scheme];
        optionsManager.controlsWindow.transform.Find("P2Controls").Find("ControlTitle").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlLabels[p2Scheme];
        optionsManager.controlsWindow.transform.Find("P2Controls").Find("ControlGuide").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlGuides[p2Scheme];

        GameObject.Find("Stages").transform.Find("Stage1").Find("p1ControlView").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlGuides[p1Scheme];
        GameObject.Find("Stages").transform.Find("Stage1").Find("p2ControlView").gameObject.GetComponent<SpriteRenderer>().sprite = optionsManager.controlGuides[p2Scheme];

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

        GameObject.Find("BGMusicSource").GetComponent<AudioSource>().volume = musicVolumeMod;
        Color origin = new Color(1, 1, 1, 1);
        closeBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - tempContrast, origin.g - tempContrast, origin.b - tempContrast, 1);
        farBG.GetComponent<SpriteRenderer>().color = new Color(origin.r - tempContrast, origin.g - tempContrast, origin.b - tempContrast, 1);
    }

    void Update() {
        if (Input.GetKeyDown("escape")) {
            ToggleOptions();
        }
    }

    bool endingGame = false;
    public void AwardPoint(bool playerNum) {

        if (!endingGame) {
            endingGame = true;
            StartCoroutine(EndGame(playerNum));
        }
    }

    public IEnumerator AwardPointWrapped(bool playerNum) {
        //playerNum is being awarded a point, and !playerNum is being awarded a perk
        Destroy(players[playerNum ? 1 : 0].gameObject);
        Destroy(players[!playerNum ? 1 : 0].gameObject);

        if (scores[playerNum ? 1 : 0] == 0) {
            scorePoints[playerNum ? 1 : 0].GetComponent<SpriteRenderer>().sprite = scoreSprites[0];
        } else if (scores[playerNum ? 1 : 0] == 1) {
            scorePoints[playerNum ? 1 : 0].GetComponent<SpriteRenderer>().sprite = scoreSprites[1];
        } else if (scores[playerNum ? 1 : 0] == 2) {
            scorePoints[playerNum ? 1 : 0].GetComponent<SpriteRenderer>().sprite = scoreSprites[2];
        }
        scores[playerNum ? 1 : 0]++;
        foreach (Transform textBox in GameObject.Find("PerkUI").transform) {
            textBox.gameObject.GetComponent<TMP_Text>().text = "";
        }
        List<string> tempPool = new List<string>(perkPool);
        for (int i = 0; i < 3; i++) {
            string perkName = tempPool[rand.Next(0, tempPool.Count)];
            //perkOptions[i].GetComponent<TMP_Text>().text = perkName;
            //perkOptionsNames[i] = perkName;
            tempPool.Remove(perkName);
        }

        Dictionary<string, string> inputDictReversed = ReverseDictionary(Player.controlSchemeData[Player.activeControlSchemes[!playerNum ? 1 : 0]]);
        int selectedPerk = 1;
        while(true) {
            if (usesController[!playerNum ? 1 : 0]) {
                //controller
                if (Input.GetButtonDown(inputDictReversed["attack"])) {
                    //playerPerks[!playerNum ? 1 : 0].Add(perkOptionsNames[selectedPerk]);
                    foreach (Transform textBox in GameObject.Find("PerkUI").transform) {
                        textBox.gameObject.GetComponent<TMP_Text>().text = "";
                    }
                    break;
                }
                if (Input.GetAxis(inputDictReversed["left"]) == -1) {
                    if (selectedPerk != 0) {
                        selectedPerk--;
                    }
                } else if (Input.GetAxis(inputDictReversed["left"]) == 1) {
                    if (selectedPerk != 2) {
                        selectedPerk++;
                    }
                }
                yield return 0;
            } else {
                //keyboard
                if (Input.GetKey(inputDictReversed["attack"])) {
                    //playerPerks[!playerNum ? 1 : 0].Add(perkOptionsNames[selectedPerk]);
                    foreach (Transform textBox in GameObject.Find("PerkUI").transform) {
                        textBox.gameObject.GetComponent<TMP_Text>().text = "";
                    }
                    break;
                }
                if (Input.GetKey(inputDictReversed["left"])) {
                    if (selectedPerk != 0) {
                        selectedPerk--;
                    }
                } else if (Input.GetKey(inputDictReversed["right"])) {
                    if (selectedPerk != 2) {
                        selectedPerk++;
                    }
                }
                yield return 0;
            }
        }


        players[!playerNum ? 1 : 0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[!playerNum ? 1 : 0].position, Quaternion.identity).GetComponent<Player>();
        players[playerNum ? 1 : 0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[playerNum ? 1 : 0].position, Quaternion.identity).GetComponent<Player>();
        players[playerNum ? 1 : 0].Init(true, p1Scheme, percentTexts[!playerNum ? 1 : 0].GetComponent<TMP_Text>(), p1Scheme == 2 ? true : false);
        players[!playerNum ? 1 : 0].Init(false, p2Scheme, percentTexts[playerNum ? 1 : 0].GetComponent<TMP_Text>(), p2Scheme == 2 ? true : false);
        yield return null;
        awardingPoint = false;
    }

    public IEnumerator EndGame(bool playerNum) {
        GameObject.Find("BGMusicSource").GetComponent<AudioSource>().Stop();
        audioSource.PlayOneShot(audioClips[6], musicVolumeMod);
        yield return new WaitForSeconds(3f);
        Destroy(players[0].gameObject);
        Destroy(players[1].gameObject);
        yield return new WaitForSeconds(0.1f);
        players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[0].Init(false, p1Scheme, percentTexts[1].GetComponent<TMP_Text>(), p1Scheme == 2 ? true : false);
        players[1].Init(true, p2Scheme, percentTexts[1].GetComponent<TMP_Text>(), p2Scheme == 2 ? true : false);
        
        scores[playerNum ? 1 : 0]++;
        if (playerNum) {
            p1ScoreDisplay.GetComponent<SpriteRenderer>().sprite = p1ScoreDisplays[scores[playerNum ? 1 : 0] - 1];
        } else {
            p2ScoreDisplay.GetComponent<SpriteRenderer>().sprite = p2ScoreDisplays[scores[playerNum ? 1 : 0] - 1];
        }
        if (scores[playerNum ? 1 : 0] == 3) {
            int count = 0;
            while (count < 20) {
                GameObject newFireWork = GameObject.Instantiate(fireWorkPrefab, new Vector3(rand.Next(100) - 50, rand.Next(15) * -1, 0), Quaternion.identity);
                newFireWork.GetComponent<Firework>().explodeHeight = rand.Next(15) + 27.5f;
                newFireWork.GetComponent<Firework>().color = playerNum;
                if (playerNum) {
                    newFireWork.GetComponent<SpriteRenderer>().sprite = p1FireWorks[0];
                } else {
                    newFireWork.GetComponent<SpriteRenderer>().sprite = p2FireWorks[0];
                }
                count++;
            }

            if (!gameWon) {
                gameWon = true;
                if (!playerNum) {
                    p1WinText.SetActive(true);
                } else {
                    p2WinText.SetActive(true);
                }
            }
        }

        if (scores[playerNum ? 1 : 0] == 1) {
            GameObject.Find("BGMusicSource").GetComponent<AudioSource>().clip = audioClips[1];
        } else if (scores[playerNum ? 1 : 0] == 2) {
            GameObject.Find("BGMusicSource").GetComponent<AudioSource>().clip = audioClips[2];
        }
        GameObject.Find("BGMusicSource").GetComponent<AudioSource>().Play();
        
        endingGame = false;
    }

    Dictionary<string, string> ReverseDictionary(Dictionary<string, string> originDict) {
        Dictionary<string, string> tempDict = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> pair in originDict) {
            tempDict[pair.Value] = pair.Key;
        }
        return tempDict;
    }

    void ToggleOptions() {
        audioSource.PlayOneShot(audioClips[3], soundeffectsVolumeMod);
        if (Time.timeScale == 1) {
            Time.timeScale = 0;
            optionsWindow.SetActive(true);
        } else {
            optionsManager.GoBack();
            Time.timeScale = 1;
            optionsWindow.SetActive(false);
        }
    }

    bool camShaking = false;
    float timer = 0.15f;

    public IEnumerator CamShake() {
        if (!camShaking) {
            camShaking = true;
            timer = 0.15f;
            while (timer > 0) {
                Vector3 origin = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
                Camera.main.transform.position = new Vector3(origin.x + 0.1f, origin.y + 0.1f, -10);
                yield return new WaitForSeconds(Time.deltaTime);
                timer -= Time.deltaTime;
                Camera.main.transform.position = new Vector3(origin.x, origin.y, -10);
                Camera.main.transform.position = new Vector3(origin.x - 0.1f, origin.y + 0.1f, -10);
                yield return new WaitForSeconds(Time.deltaTime);
                timer -= Time.deltaTime;
                Camera.main.transform.position = new Vector3(origin.x, origin.y, -10);
            }
            camShaking = false;
        } else {
            timer += 0.15f;
        }
    }

    /*bool recentlyPaused = false;
    IEnumerator ToggleOptionsBuffer() {
        recentlyPaused = true;
        yield return new WaitForSeconds(0.3f);
        recentlyPaused = false;
    }*/

    /*public void ControlsButton() {

    }*/
}
