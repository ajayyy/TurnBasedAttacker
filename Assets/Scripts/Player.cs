using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.D)) {
            transform.Translate(new Vector3(1, 0));
        }else if (Input.GetKeyDown(KeyCode.A)) {
            transform.Translate(new Vector3(-1, 0));
        }else if (Input.GetKeyDown(KeyCode.W)) {
            transform.Translate(new Vector3(0, 1));
        }else if (Input.GetKeyDown(KeyCode.S)) {
            transform.Translate(new Vector3(0, -1));
        }
    }
}
