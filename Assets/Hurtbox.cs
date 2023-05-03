using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public Player parentPlayer;
    // Start is called before the first frame update
    void Awake()
    {
        GameObject curr = gameObject;
        while (curr.tag != "Player") {
            curr = curr.transform.parent.gameObject;
        }
        parentPlayer = curr.GetComponent<Player>();
        GetComponent<BoxCollider2D>().enabled = false;
    }
}
