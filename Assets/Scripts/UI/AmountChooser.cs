using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AmountChooser : MonoBehaviour {

    public Text count;

    public int min = 1;
    public int max = 99;

    //optional, makes the maximum 10x what is in the other text field
    public Text maxText = null;

    void Start () {
		
	}

    void Update() {
        if (maxText != null) {
            max = 40 / int.Parse(maxText.text);

            if(int.Parse(count.text) > max) {
                count.text = max + "";
            }
        }
    }
	
    public void IncreaseCount() {

        if (maxText != null) {
            max = 60 / int.Parse(maxText.text);
        }

        int newNum = int.Parse(count.text) + 1;
        if (newNum < min) newNum = min;
        if (newNum > max) newNum = max;
        count.text = newNum + "";
    }

    public void DecreaseCount() {
        int newNum = int.Parse(count.text) - 1;
        if (newNum < min) newNum = min;
        if (newNum > max) newNum = max;
        count.text = newNum + "";
    }
}
