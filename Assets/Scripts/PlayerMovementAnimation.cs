using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used by the animator to allow the animation to move based on local position and to be able to move in any direction needed
/// </summary>
public class PlayerMovementAnimation : MonoBehaviour {

    //variables adjusted by animator
    public bool animating = false; //is this currently supposed to be animating
    public float offsetAmount = 0; // this amount is changed by the animator
    public float direction = 0; // direction in angles of where the object should move based on the offset

    //local variables
    Vector3 startPosition;
    bool started = false; //has animating already started or is it the first time

	void Start () {
		
	}
	
	void Update () {
        if (animating) {
            if(!started) {
                started = true;
                startPosition = transform.position;
            }

            transform.position = startPosition + MathHelper.DegreeToVector3(direction) * offsetAmount;

        }
	}

    public void OnAnimationEnded() {
        //when the animation ends, set the started to false to make this the new position
        started = false;

        //round the positions incase it didn't reach a full number for some reason
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
    }
}
