using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    public float explodeHeight;
    public bool color;

    bool exploded = false;
    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.inst;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y > explodeHeight && !exploded) {
            exploded = true;
            if (color) {
                StartCoroutine(ExplodeBlue());
            } else {
                StartCoroutine(ExplodeRed());
            }
        } else if (!exploded){
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, 0);
        }
    }

    IEnumerator ExplodeBlue() {
        GetComponent<SpriteRenderer>().sprite = gameManager.p1FireWorks[1];
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().sprite = gameManager.p1FireWorks[2];
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().sprite = gameManager.p1FireWorks[3];
        yield return new WaitForSeconds(0.5f);
        //GetComponent<SpriteRenderer>().sprite = gameManager.p1FireWorks[2];
        Destroy(gameObject);
    }

    IEnumerator ExplodeRed() {
        GetComponent<SpriteRenderer>().sprite = gameManager.p2FireWorks[1];
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().sprite = gameManager.p2FireWorks[2];
        yield return new WaitForSeconds(0.3f);
        GetComponent<SpriteRenderer>().sprite = gameManager.p2FireWorks[3];
        yield return new WaitForSeconds(0.5f);
        //GetComponent<SpriteRenderer>().sprite = gameManager.p2FireWorks[2];
        Destroy(gameObject);
    }
}
