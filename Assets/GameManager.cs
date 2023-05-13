using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    bool[] awardingPoint = new bool[]{false, false};
    GameObject restartButton;
    // Start is called before the first frame update
    void Awake()
    {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }

        players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
        players[0].Init(false, 0, percentTexts[0].GetComponent<TMP_Text>());
        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[1].Init(true, 1, percentTexts[1].GetComponent<TMP_Text>());
        
        restartButton = GameObject.Find("Canvas").transform.Find("RestartButton").gameObject;
    }

    public void AwardPoint(bool playerNum) {
        if (!playerNum && !awardingPoint[0]) {
            StartCoroutine(PointTimer1());
            if (scores[0] == 0) {
                scorePoints[0].GetComponent<SpriteRenderer>().sprite = scoreSprites[0];
            } else if (scores[0] == 1) {
                scorePoints[0].GetComponent<SpriteRenderer>().sprite = scoreSprites[1];
            } else if (scores[0] == 2) {
                scorePoints[0].GetComponent<SpriteRenderer>().sprite = scoreSprites[2];
                restartButton.SetActive(true);
            }
            scores[0]++;
            players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
            players[1].Init(true, 1, percentTexts[1].GetComponent<TMP_Text>());
            Destroy(players[0].gameObject);
            players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
            players[0].Init(false, 0, percentTexts[0].GetComponent<TMP_Text>());
        } else if (playerNum && !awardingPoint[1]){
            StartCoroutine(PointTimer2());
            if (scores[1] == 0) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[0];
            } else if (scores[1] == 1) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[1];
            } else if (scores[1] == 2) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[2];
                restartButton.SetActive(true);
            }
            scores[1]++;
            players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
            players[0].Init(false, 0, percentTexts[0].GetComponent<TMP_Text>());
            Destroy(players[1].gameObject);
            players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
            players[1].Init(true, 1, percentTexts[1].GetComponent<TMP_Text>());
        }
    }

    IEnumerator PointTimer1() {
        awardingPoint[0] = true;
        yield return new WaitForSeconds(0.1f);
        awardingPoint[0] = false;
    }
    IEnumerator PointTimer2() {
        awardingPoint[1] = true;
        yield return new WaitForSeconds(0.1f);
        awardingPoint[1] = false;
    }

    public void EndGame() {
        SceneManager.LoadScene("LucaScene");
    }
}
