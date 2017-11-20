using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    /// <summary>
    /// Which player is this, used to know if it is this player's turn
    /// </summary>
    public float playerNum = 0;

    Color highlightColor = new Color(100, 0, 0);
    Color idleColor = new Color(0, 0, 0);

    SpriteRenderer spriteRenderer;

	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    void Update () {
        //shothand for GameController.instance
        GameController gameController = GameController.instance;

        //if it is this player's turn
        if(gameController.turnPlayerNum == playerNum && Time.time - gameController.lastMove >= 0.01f) {

            spriteRenderer.color = highlightColor;

            bool moved = false;

            if (Input.GetKeyDown(KeyCode.D)) {
                transform.Translate(new Vector3(1, 0));
                moved = true;
            } else if (Input.GetKeyDown(KeyCode.A)) {
                transform.Translate(new Vector3(-1, 0));
                moved = true;
            } else if (Input.GetKeyDown(KeyCode.W)) {
                transform.Translate(new Vector3(0, 1));
                moved = true;
            } else if (Input.GetKeyDown(KeyCode.S)) {
                transform.Translate(new Vector3(0, -1));
                moved = true;
            }

            if (moved) {
                gameController.NextTurn();
                spriteRenderer.color = idleColor;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        print("test");
    }

}
