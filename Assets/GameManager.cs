using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager inst = null;
    public System.Random rand = new System.Random();
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints = new Transform[2];

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
        players[0].Init(false, "WASD");
        players[1] = GameObject.Instantiate(playerPrefab, playerSpawnPoints[1].position, Quaternion.identity).GetComponent<Player>();
        players[1].Init(true, "ArrowKeys");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
