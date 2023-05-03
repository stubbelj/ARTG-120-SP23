using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player parentPlayer;
    public AttackData attackData;
    //loaded by Player on Awake()

    void Awake()
    {
        GameObject curr = gameObject;
        while (curr.tag != "Player") {
            curr = curr.transform.parent.gameObject;
        }
        parentPlayer = curr.GetComponent<Player>();
        GetComponent<BoxCollider2D>().enabled = false;
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Hurtbox") {
            if (col.gameObject.GetComponent<Hurtbox>().parentPlayer != parentPlayer.gameObject) {
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
            }
        }
    }
}
