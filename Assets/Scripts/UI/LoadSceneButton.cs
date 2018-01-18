using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneButton : MonoBehaviour {

    public string sceneToLoad = "";

	void Start () {
		
	}
	
	void Update () {
    }

    public void OnClick() {
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
}
