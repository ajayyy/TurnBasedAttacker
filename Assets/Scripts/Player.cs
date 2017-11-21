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
    PlayerShootAnimation playerShootAnimation;
    Animator animator;

    public GameObject projectile;

    bool doneTurn = false; //if true, and the animation state is idle, then go to next turn

    //info on what the player is holding
    int pickup = 0;
    bool holding = false; //true when holding

	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();

        playerMovementAnimation = GetComponent<PlayerMovementAnimation>();
        playerShootAnimation = projectile.GetComponent<PlayerShootAnimation>();
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
                int movementType = 0; // if moved, then what type of move was it (atually moving, firing a projectile, etc.)
                //0 is move, 1 is fire

                //movement
                if (Input.GetKeyDown(KeyCode.D)) {
                    playerMovementAnimation.direction = 0;
                    moved = true;
                    movementType = 0;
                } else if (Input.GetKeyDown(KeyCode.A)) {
                    playerMovementAnimation.direction = 180;
                    moved = true;
                    movementType = 0;
                } else if (Input.GetKeyDown(KeyCode.W)) {
                    playerMovementAnimation.direction = 90;
                    moved = true;
                    movementType = 0;
                } else if (Input.GetKeyDown(KeyCode.S)) {
                    playerMovementAnimation.direction = 270;
                    moved = true;
                    movementType = 0;
                }

                //projectiles
                if (Input.GetKeyDown(KeyCode.E)) {
                    playerShootAnimation.direction = 0;
                    moved = true;
                    movementType = 1;
                }

                if (moved) {
                    switch (movementType) {
                        case 0:
                            animator.SetTrigger("move");
                            doneTurn = true; //once the animation becomes idle again, the doneTurn if statement will be triggered, and the next turn will start
                            break;
                        case 1:
                            projectile.GetComponent<Animator>().SetTrigger("move");
                            doneTurn = true;
                            break;
                    }
                }
            }

        }

    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag.Equals("Pickup")) {
            switch (collider.GetComponent<Pickup>().type) {
                case 0:
                    //TODO display some marker on the player that it can now shoot a projectile
                    pickup = 0;
                    holding = true;
                    break;
            }

            Destroy(collider.gameObject);
        }
    }

}
