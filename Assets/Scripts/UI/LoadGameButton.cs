using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadGameButton : MonoBehaviour {

    public int gameNum = 0;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void OnClick() {
        GameSettings.players = 0;
        GameSettings.units = 0;

        GameSettings.gameToLoad = gameNum;

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}
