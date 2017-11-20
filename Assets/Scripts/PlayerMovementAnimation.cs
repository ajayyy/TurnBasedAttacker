using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used by the animator to allow the animation to move based on local position and to be able to move in any direction needed
/// </summary>
public class PlayerMovementAnimation : MonoBehaviour {

    bool animating = false; //is this currently supposed to be animating

    public float offsetAmount = 0; // this amount is changed by the animator

    public float direction = 0; // direction in angles of where the object should move based on the offset

	void Start () {
		
	}
	
	void Update () {
        if (animating) {

        }
	}
}
