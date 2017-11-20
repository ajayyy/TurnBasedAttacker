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

    PlayerMovementAnimation playerMovementAnimation;
    Animator animator;

    bool doneTurn = false; //if true, and the animation state is idle, then go to next turn

	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();

        playerMovementAnimation = GetComponent<PlayerMovementAnimation>();
        animator = GetComponent<Animator>();
    }

    void Update () {
        //shothand for GameController.instance
        GameController gameController = GameController.instance;

        //if it is this player's turn
        if(gameController.turnPlayerNum == playerNum && Time.time - gameController.lastMove >= 0.01f && animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
            if (doneTurn) {
                gameController.NextTurn();
                spriteRenderer.color = idleColor;
                doneTurn = false;
            }else {
                spriteRenderer.color = highlightColor;

                bool moved = false;

                if (Input.GetKeyDown(KeyCode.D)) {
                    playerMovementAnimation.direction = 0;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    playerMovementAnimation.direction = 180;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    playerMovementAnimation.direction = 90;
                    moved = true;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    playerMovementAnimation.direction = 270;
                    moved = true;
                }

                if (moved) {
                    animator.SetTrigger("move");
                    doneTurn = true;
                }
            }

        }

    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag.Equals("Pickup")) {
            switch (collider.GetComponent<Pickup>().type) {
                case 0:
                    //TODO display some marker on the player that it can now shoot a projectile
                    break;
            }

            Destroy(collider.gameObject);
        }
    }

}
