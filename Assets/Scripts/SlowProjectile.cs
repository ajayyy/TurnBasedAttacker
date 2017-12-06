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

	void Start () {
        animationScript = GetComponent<AnimationScript>();

        animator = GetComponent<Animator>();
    }
	
	void FixedUpdate () {
        GameController gameController = GameController.instance;

		if(lastTurnMoved != gameController.turnPlayerNum) {

            lastTurnMoved = gameController.turnPlayerNum;

            animationScript.direction = direction;
            animator.SetTrigger("move");
        }
	}
}
