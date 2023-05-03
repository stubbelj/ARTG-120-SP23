using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst = null;
    public System.Random rand = new System.Random();
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints = new Transform[2];
    public GameObject[] scorePoints = new GameObject[2];
    public Sprite[] scoreSprites = new Sprite[3];
    public int[] scores = new int[]{0, 0};
    bool[] awardingPoint = new bool[]{false, false};

    Player[] players = new Player[2];
    // Start is called before the first frame update
    void Awake()
    {
        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }

        players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
        players[0].Init(false, 0);
        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[1].Init(true, 1);
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
            }
            scores[0]++;
            players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
            players[1].Init(true, 1);
        } else if (playerNum && !awardingPoint[1]){
            StartCoroutine(PointTimer2());
            if (scores[1] == 0) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[0];
            } else if (scores[1] == 1) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[1];
            } else if (scores[1] == 2) {
                scorePoints[1].GetComponent<SpriteRenderer>().sprite = scoreSprites[2];
            }
            scores[1]++;
            players[0] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity).GetComponent<Player>();
            players[0].Init(false, 0);
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
}
