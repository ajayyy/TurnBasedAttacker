using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AmountChooser : MonoBehaviour {

    public Text count;

    public int min = 1;
    public int max = 99;

    void Start () {
		
	}
	
    public void IncreaseCount() {
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
