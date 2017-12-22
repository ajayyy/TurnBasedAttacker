using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StartButtonScript : MonoBehaviour {

    public Text players;
    public Text units;

	void Start () {
		
	}
	
	void Update () {
		
	}

    public void OnClick() {
        GameSettings.players = int.Parse(players.text);
        GameSettings.units = int.Parse(units.text);

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}
