using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player parentPlayer;
    public AttackData attackData;
    AttackData blockHitData;
    //loaded by Player on Awake()

    void Awake()
    {
        GameObject curr = gameObject;
        while (curr.tag != "Player") {
            curr = curr.transform.parent.gameObject;
        }
        parentPlayer = curr.GetComponent<Player>();
        GetComponent<BoxCollider2D>().enabled = false;
        blockHitData = new AttackData(0, false, parentPlayer, AttackData.attackIdFlow++);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Hurtbox") {
            if (col.gameObject.GetComponent<Hurtbox>().parentPlayer != parentPlayer.gameObject) {
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
            }
        } else if (col.gameObject.tag == "BlockHurtbox") {
            parentPlayer.TakeDamage(blockHitData, col.ClosestPoint(transform.position));
            col.gameObject.GetComponent<Hurtbox>().parentPlayer.EndBlock();
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (col.gameObject.tag == "Hurtbox") {
            if (col.gameObject.GetComponent<Hurtbox>().parentPlayer != parentPlayer.gameObject) {
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
            }
        } else if (col.gameObject.tag == "BlockHurtbox") {
            parentPlayer.TakeDamage(blockHitData, col.ClosestPoint(transform.position));
            col.gameObject.GetComponent<Hurtbox>().parentPlayer.EndBlock();
        }
    }
}