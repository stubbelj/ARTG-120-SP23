using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player parentPlayer;
    public AttackData attackData;
    AttackData blockHitData;
    bool throwing = false;
    //loaded by Player on Awake()

    void Awake()
    {
        GameObject curr = gameObject;
        while (curr.tag != "Player") {
            curr = curr.transform.parent.gameObject;
        }
        parentPlayer = curr.GetComponent<Player>();
        GetComponent<BoxCollider2D>().enabled = false;
        blockHitData = new AttackData(0, 1f, false, 0f, parentPlayer, AttackData.attackIdFlow++);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (gameObject.tag == "GrabHitbox") {
            if (col.gameObject.tag == "BlockHurtbox" || col.gameObject.tag == "Hurtbox") {
                if (!throwing) {
                    col.gameObject.GetComponent<Hurtbox>().parentPlayer.BeginThrown();
                    StopCoroutine(parentPlayer.currActionCoroutine);
                    parentPlayer.currActionCoroutine = null;
                    parentPlayer.currActionCoroutine = StartCoroutine(parentPlayer.Throw());
                    throwing = true;
                    StartCoroutine(ThrowingReset());
                }
            }
        } else {
            if (col.gameObject.tag == "BlockHurtbox") {
                parentPlayer.TakeDamage(blockHitData, col.ClosestPoint(transform.position));
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.EndBlock();
            } else if (col.gameObject.tag == "Hurtbox") {
                if (col.gameObject.GetComponent<Hurtbox>().parentPlayer != parentPlayer.gameObject) {
                    col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
                }
            }
        }
    }

    void OnTriggerStay2D(Collider2D col) {
        if (gameObject.tag == "GrabHitbox") {
            if (col.gameObject.tag == "BlockHurtbox" || col.gameObject.tag == "Hurtbox") {
                if (!throwing) {
                    col.gameObject.GetComponent<Hurtbox>().parentPlayer.BeginThrown();
                    StopCoroutine(parentPlayer.currActionCoroutine);
                    parentPlayer.currActionCoroutine = null;
                    parentPlayer.currActionCoroutine = StartCoroutine(parentPlayer.Throw());
                    throwing = true;
                    StartCoroutine(ThrowingReset());
                }
            }
        } else {
            if (col.gameObject.tag == "BlockHurtbox") {
                parentPlayer.TakeDamage(blockHitData, col.ClosestPoint(transform.position));
                col.gameObject.GetComponent<Hurtbox>().parentPlayer.EndBlock();
            } else if (col.gameObject.tag == "Hurtbox") {
                if (col.gameObject.GetComponent<Hurtbox>().parentPlayer != parentPlayer.gameObject) {
                    col.gameObject.GetComponent<Hurtbox>().parentPlayer.TakeDamage(attackData, col.ClosestPoint(transform.position));
                }
            }
        }
    }

    IEnumerator ThrowingReset() {
        yield return new WaitForSeconds(0.3f);
        throwing = false;
    }
}