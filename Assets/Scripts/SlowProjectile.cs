using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectile : MonoBehaviour {

    AnimationScript animationScript;
    Animator animator;

    //variables set by player class

    //how much it moves per turn
    public int movementStep = 1;
    public float direction;

    //local variables

    public int lastTurnMoved = -1; //which players turn it was

    bool firstTurn = true; //set to true in ontriggerenter. Used to determine whether to do anything since it does not want to kill the player right when the throw it

    bool dead = false; //true if dying on next frame

    void Start() {
        animationScript = GetComponent<AnimationScript>();

        animator = GetComponent<Animator>();
    }

    void FixedUpdate() {
        print(animator.GetCurrentAnimatorStateInfo(0).IsName("idle"));
        if (dead && animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
            Destroy(gameObject);
            return;
        }

        //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) {
        //    RaycastHit2D otherObject = Physics2D.Raycast(transform.position, MathHelper.DegreeToVector2(direction));

        //    if (otherObject.collider != null && Vector3.Distance(otherObject.point, transform.position) < 1f || otherObject.collider.tag.Equals("Border")) {
        //        dead = true;
        //    }
        //}

        GameController gameController = GameController.instance;

        if (lastTurnMoved != gameController.turnPlayerNum) {

            lastTurnMoved = gameController.turnPlayerNum;

            animationScript.direction = direction;
            animator.SetTrigger("move");
        }

    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (firstTurn) {
            firstTurn = false;
            return;
        }

        if (collider.gameObject.tag == "Player" && lastTurnMoved != -1) {
            collider.GetComponent<Animator>().SetTrigger("dead");

            dead = true;
        }

        if (collider.gameObject.tag == "Block") {
            collider.GetComponent<Animator>().SetTrigger("dead");

            dead = true;
        }

        if(collider.gameObject.tag == "Border") {
            GetComponent<Animator>().SetTrigger("dead");
            dead = true;
        }
    }
}
