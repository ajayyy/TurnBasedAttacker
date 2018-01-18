using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LoadGameList : MonoBehaviour {

    public GameObject canvas;

    //button prefab
    public GameObject button;

	void Start () {
        int gameAmount = PlayerPrefs.GetInt("GameAmount");

        for(int i = 0; i < gameAmount; i++) {
            GameObject newButton = Instantiate(button);

            newButton.transform.parent = canvas.transform;
            newButton.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 360 - ((i + 1) * 75));

            newButton.GetComponentInChildren<Text>().text = "Game " + (i + 1);

            newButton.GetComponent<LoadGameButton>().gameNum = i;
        }

    }

    void Update () {
		
	}
}
