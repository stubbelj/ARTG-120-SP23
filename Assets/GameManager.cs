using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
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
    GameObject[] perkOptions = new GameObject[3];
    string[] perkOptionsNames = new string[3];
    List<string> perkPool = new List<string>{
        "superSpeed", "superSpeed", "superSpeed"
    };

    bool awardingPoint = false;
    // Start is called before the first frame update
    void Awake() {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }

        players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
        players[0].Init(false, 0, percentTexts[0].GetComponent<TMP_Text>(), false);
        Player.activeControlSchemes[0] = 0;
        usesController[0] = false;
        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[1].Init(true, 1, percentTexts[1].GetComponent<TMP_Text>(), true);
        usesController[1] = true;
        Player.activeControlSchemes[1] = 2;

        perkOptions[0] = GameObject.Find("PerkUI").transform.Find("PerkOption0").gameObject;
        perkOptions[1] = GameObject.Find("PerkUI").transform.Find("PerkOption1").gameObject;
        perkOptions[2] = GameObject.Find("PerkUI").transform.Find("PerkOption2").gameObject;
    }

    void Update() {
    }

    public void AwardPoint(bool playerNum) {
        if (!awardingPoint) {
            awardingPoint = true;
            StartCoroutine(AwardPointWrapped(playerNum));
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
            perkOptions[i].GetComponent<TMP_Text>().text = perkName;
            perkOptionsNames[i] = perkName;
            tempPool.Remove(perkName);
        }

        Dictionary<string, string> inputDictReversed = ReverseDictionary(Player.controlSchemeData[Player.activeControlSchemes[!playerNum ? 1 : 0]]);
        int selectedPerk = 1;
        while(true) {
            if (usesController[!playerNum ? 1 : 0]) {
                //controller
                if (Input.GetButtonDown(inputDictReversed["attack"])) {
                    playerPerks[!playerNum ? 1 : 0].Add(perkOptionsNames[selectedPerk]);
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
                    playerPerks[!playerNum ? 1 : 0].Add(perkOptionsNames[selectedPerk]);
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
        players[playerNum ? 1 : 0].Init(true, 1, percentTexts[!playerNum ? 1 : 0].GetComponent<TMP_Text>(), true);
        players[!playerNum ? 1 : 0].Init(false, 0, percentTexts[playerNum ? 1 : 0].GetComponent<TMP_Text>(), false);
        yield return null;
        awardingPoint = false;
    }

    public void EndGame() {
        SceneManager.LoadScene("LucaScene");
    }

    Dictionary<string, string> ReverseDictionary(Dictionary<string, string> originDict) {
        Dictionary<string, string> tempDict = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> pair in originDict) {
            tempDict[pair.Value] = pair.Key;
        }
        return tempDict;
    }
}
