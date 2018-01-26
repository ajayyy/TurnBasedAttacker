using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData {
    public Vector2 position = Vector2.zero;

    public int playerNum;

    public PlayerData(Vector2 position, int playerNum) {
        this.position = position;
        this.playerNum = playerNum;
    }
}
